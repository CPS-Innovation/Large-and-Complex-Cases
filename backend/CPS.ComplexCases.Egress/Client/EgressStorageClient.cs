using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;

namespace CPS.ComplexCases.Egress.Client;

public class EgressStorageClient(
    ILogger<EgressStorageClient> logger,
    IOptions<EgressOptions> egressOptions,
    HttpClient httpClient,
    IEgressRequestFactory egressRequestFactory,
    ITelemetryClient telemetryClient) : BaseEgressClient(logger, egressOptions, httpClient, egressRequestFactory, telemetryClient), IStorageClient
{
    private const string RootPathValue = ".";

    public async Task<(Stream Stream, long ContentLength)> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null, string? bearerToken = null, string? bucketName = null)
    {
        var token = await GetWorkspaceToken();

        var arg = new GetWorkspaceDocumentArg
        {
            WorkspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            FileId = fileId ?? throw new ArgumentNullException(nameof(fileId), "File ID cannot be null."),
        };

        var response = await SendRequestAsync(_egressRequestFactory.GetWorkspaceDocumentRequest(arg, token), true);
        var contentLength = response.Content.Headers.ContentLength ?? -1;

        var stream = await response.Content.ReadAsStreamAsync();

        return (stream, contentLength);
    }

    public async Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string sourcePath, string? workspaceId = null, string? relativePath = null, string? sourceRootFolderPath = null, string? bearerToken = null, string? bucketName = null)
    {
        var token = await GetWorkspaceToken();

        if (string.IsNullOrEmpty(relativePath))
            throw new ArgumentNullException(nameof(relativePath), "Relative path cannot be null or empty.");

        var fileName = Path.GetFileName(relativePath);

        var fullDestinationPath = GetDestinationFolderPath(destinationPath, relativePath, sourceRootFolderPath);

        var arg = new CreateUploadArg
        {
            FolderPath = fullDestinationPath,
            FileSize = fileSize,
            WorkspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            FileName = fileName,
        };

        try
        {
            var response = await SendRequestAsync<CreateUploadResponse>(_egressRequestFactory.CreateUploadRequest(arg, token));

            return new UploadSession
            {
                UploadId = response.Id,
                WorkspaceId = workspaceId,
                Md5Hash = response.Md5Hash,
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound || ex.Message.Contains("404"))
        {
            _logger.LogInformation("Folder structure doesn't exist for path {FolderPath}, creating it", fullDestinationPath);

            await CreateFolderStructureAsync(fullDestinationPath, workspaceId, token);

            // The newly created folder is not always immediately visible to create-upload, so retry
            // the create-upload a few times with a short settle delay rather than a single bare retry.
            var maxAttempts = Math.Max(1, _egressOptions.CreateUploadRetryAttempts);
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                await Task.Delay(GetCreateUploadSettleDelay(attempt));

                try
                {
                    var response = await SendRequestAsync<CreateUploadResponse>(_egressRequestFactory.CreateUploadRequest(arg, token));
                    return new UploadSession
                    {
                        UploadId = response.Id,
                        WorkspaceId = workspaceId,
                        Md5Hash = response.Md5Hash,
                    };
                }
                catch (HttpRequestException retryEx) when (attempt < maxAttempts
                    && (retryEx.StatusCode == HttpStatusCode.NotFound || retryEx.Message.Contains("404")))
                {
                    _logger.LogWarning(retryEx,
                        "Create-upload still returns 404 after folder creation for {FolderPath} (attempt {Attempt}/{MaxAttempts}); retrying after settle delay.",
                        fullDestinationPath, attempt, maxAttempts);
                }
            }

            throw new InvalidOperationException(
                $"Create-upload still returns 404 after folder creation for {fullDestinationPath} after {maxAttempts} attempts.");
        }
    }

    // Computes the Egress destination folder (directory) for a source file's relative path, mirroring
    // the path logic used by create-upload so pre-creation and create-upload target the same folder.
    public static string GetDestinationFolderPath(string destinationPath, string? relativePath, string? sourceRootFolderPath)
    {
        var relativePathFromSourceRoot = GetRelativePathFromSourceRoot(relativePath ?? string.Empty, sourceRootFolderPath);
        var sourceDirectory = Path.GetDirectoryName(relativePathFromSourceRoot) ?? string.Empty;
        return Path.Combine(destinationPath, sourceDirectory).Replace('\\', '/');
    }

    private TimeSpan GetCreateUploadSettleDelay(int attempt)
    {
        var baseSeconds = Math.Max(1, _egressOptions.CreateUploadSettleDelaySeconds);
        return TimeSpan.FromSeconds(baseSeconds * attempt);
    }

    public async Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, long? start = null, long? end = null, long? totalSize = null, string? bearerToken = null, string? bucketName = null)
    {
        var token = await GetWorkspaceToken();

        var uploadArg = new UploadChunkArg
        {
            UploadId = session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            WorkspaceId = session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            ChunkData = chunkData,
            Start = start,
            End = end,
            TotalSize = totalSize
        };

        var maxAttempts = Math.Max(1, _egressOptions.MaxChunkUploadAttempts);
        var transferTimeout = TimeSpan.FromSeconds(_egressOptions.TransferTimeoutSeconds);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                // Rebuild the request on every attempt: the chunk body is MultipartFormDataContent,
                // which cannot be re-sent, so a retry must construct a fresh HttpRequestMessage.
                await SendRequestAsync(
                    _egressRequestFactory.UploadChunkRequest(uploadArg, token),
                    timeout: transferTimeout);

                return new UploadChunkResult(TransferDirection.NetAppToEgress);
            }
            catch (Exception ex) when (attempt < maxAttempts && IsRetryableChunkError(ex))
            {
                var delay = GetChunkRetryDelay(attempt);
                _logger.LogWarning(ex,
                    "Chunk {ChunkNumber} upload for upload {UploadId} failed with a retryable error (attempt {Attempt}/{MaxAttempts}). Retrying in {DelayMs}ms.",
                    chunkNumber, session.UploadId, attempt, maxAttempts, delay.TotalMilliseconds);
                await Task.Delay(delay);
            }
        }

        throw new InvalidOperationException(
            $"Chunk {chunkNumber} upload for upload {session.UploadId} failed after {maxAttempts} attempts.");
    }

    // Egress chunk PATCH's fail intermittently with 5xx/429 under load, and a slow link can trip the
    // per-chunk timeout. All of these are worth a bounded retry before failing the whole file.
    private static bool IsRetryableChunkError(Exception ex) =>
        (ex is HttpRequestException httpEx
            && ((int?)httpEx.StatusCode >= 500 || httpEx.StatusCode == HttpStatusCode.TooManyRequests))
        || ex is OperationCanceledException
        || ex is TimeoutException;

    // Exponential backoff with jitter, so concurrent chunk retries do not synchronise into a storm.
    private TimeSpan GetChunkRetryDelay(int attempt)
    {
        var baseSeconds = Math.Max(1, _egressOptions.ChunkRetryBaseDelaySeconds);
        var exponentialSeconds = baseSeconds * Math.Pow(2, attempt - 1);
        var jitterMs = Random.Shared.Next(0, 1000);
        return TimeSpan.FromSeconds(exponentialSeconds) + TimeSpan.FromMilliseconds(jitterMs);
    }

    public async Task<bool> CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null, string? bearerToken = null, string? bucketName = null, string? filePath = null)
    {
        var token = await GetWorkspaceToken();

        var completeArg = new CompleteUploadArg
        {
            UploadId = session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            WorkspaceId = session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            Md5Hash = md5hash,
        };

        var response = await SendRequestAsync(_egressRequestFactory.CompleteUploadRequest(completeArg, token));
        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null, string? bearerToken = null, string? bucketName = null)
    {
        if (selectedEntities == null || selectedEntities.Count == 0)
            throw new ArgumentException("Selected entities cannot be null or empty", nameof(selectedEntities));

        if (string.IsNullOrEmpty(workspaceId))
        {
            throw new ArgumentException("Workspace ID cannot be null or empty.", nameof(workspaceId));
        }

        var token = await GetWorkspaceToken();

        var entityTasks = selectedEntities.Select(async entity =>
        {
            if (entity.IsFolder != true)
            {
                return new List<FileTransferInfo>
                {
                    new FileTransferInfo
                    {
                        Id = entity.FileId,
                        SourcePath = Path.GetFileName(entity.Path),
                        FullFilePath = entity.Path,
                    }
                };
            }
            else
            {
                return await GetAllFilesFromFolderParallel(workspaceId, entity.FileId, entity.Path, token);
            }
        });

        var results = await Task.WhenAll(entityTasks);
        return results.SelectMany(files => files);
    }

    public async Task<DeleteFilesResult> DeleteFilesAsync(List<DeletionEntityDto> filesToDelete, string? workspaceId = null, string? bearerToken = null, string? bucketName = null)
    {
        if (filesToDelete == null || filesToDelete.Count == 0)
            throw new ArgumentException("Selected entities cannot be null or empty", nameof(filesToDelete));

        if (string.IsNullOrEmpty(workspaceId))
        {
            throw new ArgumentException("Workspace ID cannot be null or empty.", nameof(workspaceId));
        }

        var token = await GetWorkspaceToken();

        var fileIds = filesToDelete.Select(f => f.FileId).ToList();

        if (fileIds.Count == 0)
        {
            _logger.LogInformation("No files to delete for workspace ID {WorkspaceId}.", workspaceId);
            return new DeleteFilesResult();
        }

        var deleteArg = new DeleteFilesArg
        {
            WorkspaceId = workspaceId,
            FileIds = fileIds!
        };

        var result = await SendRequestAsync<DeleteFilesResponse>(_egressRequestFactory.DeleteFilesRequest(deleteArg, token));

        return new DeleteFilesResult
        {
            DeletedFiles = result.Files.Select(x => x.FileId).Where(id => id != null).Cast<string>().ToList(),
            FailedFiles = result.Files.Where(x => x.Code > 0).Select(x => new FailedFileDeletion
            {
                FileId = x.FileId ?? string.Empty,
                Filename = x.Filename ?? string.Empty,
                Reason = x.Status ?? string.Empty
            })
            .ToList()
        };
    }

    public Task UploadFileAsync(string destinationPath, Stream fileStream, long contentLength, string? workspaceId = null, string? relativePath = null, string? sourceRootFolderPath = null, string? bearerToken = null, string? bucketName = null)
    {
        // This shares an interface with NetAppStorageClient but isn't required for Egress
        // Egress always uses chunked uploads via InitiateUploadAsync, UploadChunkAsync, and CompleteUploadAsync
        throw new NotImplementedException();
    }

    public async Task<bool> FileExistsAsync(string path, string? workspaceId = null, string? bearerToken = null, string? bucketName = null)
    {
        var token = await GetWorkspaceToken();

        var existingFiles = await GetAllFilesFromFolderParallel(
            workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            "",
            "",
            token);

        return existingFiles.Any(f => !string.IsNullOrEmpty(f.FullFilePath) && f.FullFilePath.Equals(path, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<FileTransferInfo>> GetAllFilesFromFolderAsync(string folderPath, string? workspaceId = null)
    {
        return await GetAllFilesFromFolderParallel(
            workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            "",
            folderPath,
            await GetWorkspaceToken());
    }

    public async Task<bool> CreateFolderAsync(string folderPath, string? workspaceId = null, string? bearerToken = null, string? bucketName = null)
    {
        if (string.IsNullOrEmpty(workspaceId))
            throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null.");

        var token = await GetWorkspaceToken();

        // CreateFolderStructureAsync creates each missing segment and swallows folder-level 409s, so
        // this is idempotent and safe to call once up front before the transfer fans out.
        await CreateFolderStructureAsync(folderPath, workspaceId, token);
        return true;
    }

    private async Task CreateFolderStructureAsync(string folderPath, string workspaceId, string token)
    {
        if (string.IsNullOrEmpty(folderPath) || folderPath == "/" || folderPath == "\\")
            return;

        _logger.LogDebug("Creating folder structure for path: {FolderPath}", folderPath);

        var pathSegments = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentPath = "";

        foreach (var segment in pathSegments)
        {
            currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}/{segment}";
            var wasCreated = await TryCreateFolderAsync(currentPath, workspaceId, token);

            if (wasCreated)
            {
                _logger.LogDebug("Successfully created folder: {FolderPath}", currentPath);
            }
        }

        _logger.LogInformation("Completed folder structure creation for path: {FolderPath}", folderPath);
    }

    private async Task<bool> TryCreateFolderAsync(string folderPath, string workspaceId, string token)
    {
        var parentPath = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? "";
        var folderName = Path.GetFileName(folderPath);

        var arg = new CreateFolderArg
        {
            WorkspaceId = workspaceId,
            FolderName = folderName,
            Path = parentPath
        };

        if (string.IsNullOrEmpty(arg.Path)) return false;

        try
        {
            var request = _egressRequestFactory.CreateFolderRequest(arg, token);

            // The storage HttpClient has an infinite timeout, so this direct send (which bypasses
            // SendRequestAsync) needs its own management-scoped timeout.
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_egressOptions.ManagementTimeoutSeconds));
            var response = await _httpClient.SendAsync(request, timeoutCts.Token);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                // Folder already exists - this is expected and OK
                _logger.LogDebug("Folder {FolderPath} already exists", folderPath);
                return false;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create folder {FolderPath}. Status: {StatusCode}, Response: {Response}",
                folderPath, response.StatusCode, errorContent);

            response.EnsureSuccessStatusCode();
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while creating folder {FolderPath}", folderPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating folder {FolderPath}", folderPath);
            throw;
        }
    }

    private async Task<List<FileTransferInfo>> GetAllFilesFromFolderParallel(string workspaceId, string? folderId, string baseFolderPath, string token)
    {
        var allPagesData = await GetAllPagesInParallel(workspaceId, folderId, token);
        var files = allPagesData
            .Where(d => !d.IsFolder)
            .Select(d => new FileTransferInfo
            {
                Id = d.Id,
                SourcePath = ConstructRelativePath(baseFolderPath, d.Path, d.FileName),
                FullFilePath = d.Path.EnsureTrailingSlash() + d.FileName
            })
            .ToList();
        var folders = allPagesData.Where(d => d.IsFolder).ToList();
        if (folders.Any())
        {
            var subFolderTasks = folders.Select(folder =>
                GetAllFilesFromFolderParallel(workspaceId, folder.Id, baseFolderPath, token));
            var subFolderResults = await Task.WhenAll(subFolderTasks);
            files.AddRange(subFolderResults.SelectMany(x => x));
        }
        return files;
    }

    private static string ConstructRelativePath(string baseFolderPath, string filePath, string fileName)
    {
        // A file at the workspace root has an empty path. Path.GetRelativePath throws on an empty
        // path, and a recursive listing (folderId "") can legitimately surface such root-level
        // entries, so there is no sub-structure to preserve — fall back to the file name.
        if (string.IsNullOrEmpty(filePath))
        {
            return fileName;
        }

        // Get the folder name from the base path (selected folder)
        var baseFolderName = Path.GetFileName(baseFolderPath.TrimEnd('/'));

        if (string.IsNullOrEmpty(baseFolderName))
        {
            return Path.Combine(filePath, fileName).Replace('\\', '/');
        }

        var relativePath = Path.GetRelativePath(baseFolderPath, filePath).Replace('\\', '/');

        if (relativePath == RootPathValue)
        {
            return Path.Combine(baseFolderName, fileName).Replace('\\', '/');
        }
        else
        {
            // Combine the selected folder and subfolder structure with the file name
            return Path.Combine(baseFolderName, relativePath, fileName).Replace('\\', '/');
        }
    }

    private async Task<List<ListCaseMaterialDataResponse>> GetAllPagesInParallel(string workspaceId, string? folderId, string token)
    {
        const int take = 100;

        var initialArg = new ListWorkspaceMaterialArg
        {
            WorkspaceId = workspaceId,
            FolderId = folderId,
            Take = take,
            Skip = 0,
            RecurseSubFolders = false
        };

        var initialResponse = await SendRequestAsync<ListCaseMaterialResponse>(_egressRequestFactory.ListEgressMaterialRequest(initialArg, token));
        var totalResults = initialResponse.DataInfo.TotalResults;
        var allData = new List<ListCaseMaterialDataResponse>(initialResponse.Data);

        if (totalResults > take)
        {
            int batchSize = 10;

            var remainingPages = (int)Math.Ceiling((double)(totalResults - take) / take);
            var pageTasks = new List<Task<ListCaseMaterialResponse>>();
            ListCaseMaterialResponse[]? pageResults = [];

            for (int i = 1; i <= remainingPages; i++)
            {
                var pageArg = new ListWorkspaceMaterialArg
                {
                    WorkspaceId = workspaceId,
                    FolderId = folderId,
                    Take = take,
                    Skip = i * take,
                    RecurseSubFolders = false,
                    ViewFullDetails = true
                };

                pageTasks.Add(SendRequestAsync<ListCaseMaterialResponse>(_egressRequestFactory.ListEgressMaterialRequest(pageArg, token)));

                if (pageTasks.Count >= batchSize)
                {
                    pageResults = await Task.WhenAll(pageTasks);
                    allData.AddRange(pageResults.SelectMany(r => r.Data));
                    pageTasks.Clear();
                }
            }

            if (pageTasks.Count > 0)
            {
                pageResults = await Task.WhenAll(pageTasks);
                allData.AddRange(pageResults.SelectMany(r => r.Data));
            }
        }

        return allData;
    }

    internal static string GetRelativePathFromSourceRoot(string relativePath, string? sourceRootFolderPath)
    {
        return !string.IsNullOrEmpty(sourceRootFolderPath) && relativePath.StartsWith(sourceRootFolderPath, StringComparison.OrdinalIgnoreCase) ?
            relativePath.Substring(sourceRootFolderPath.Length).TrimStart('/', '\\') :
            relativePath;
    }
}