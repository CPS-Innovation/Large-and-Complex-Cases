using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;

namespace CPS.ComplexCases.Egress.Client;

public class EgressStorageClient(
    ILogger<EgressStorageClient> logger,
    IOptions<EgressOptions> egressOptions,
    HttpClient httpClient,
    IEgressRequestFactory egressRequestFactory) : BaseEgressClient(logger, egressOptions, httpClient, egressRequestFactory), IStorageClient
{
    public async Task<Stream> OpenReadStreamAsync(string path, string? workspaceId, string? fileId)
    {
        var token = await GetWorkspaceToken();

        var arg = new GetWorkspaceDocumentArg
        {
            WorkspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            FileId = fileId ?? throw new ArgumentNullException(nameof(fileId), "File ID cannot be null."),
        };

        var response = await SendRequestAsync(_egressRequestFactory.GetWorkspaceDocumentRequest(arg, token));
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string sourcePath, string? workspaceId = null, string? relativePath = null)
    {
        var token = await GetWorkspaceToken();

        if (string.IsNullOrEmpty(relativePath))
            throw new ArgumentNullException(nameof(relativePath), "Relative path cannot be null or empty.");

        var fileName = Path.GetFileName(relativePath);
        var sourceDirectory = Path.GetDirectoryName(relativePath) ?? string.Empty;
        var fullDestinationPath = Path.Combine(destinationPath, sourceDirectory).Replace('\\', '/');

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
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogInformation("Folder structure doesn't exist for path {FolderPath}, creating it", fullDestinationPath);

            await CreateFolderStructureAsync(fullDestinationPath, workspaceId, token);

            var response = await SendRequestAsync<CreateUploadResponse>(_egressRequestFactory.CreateUploadRequest(arg, token));
            return new UploadSession
            {
                UploadId = response.Id,
                WorkspaceId = workspaceId,
                Md5Hash = response.Md5Hash,
            };
        }
    }

    public async Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, string? contentRange = null)
    {
        var token = await GetWorkspaceToken();

        var uploadArg = new UploadChunkArg
        {
            UploadId = session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            WorkspaceId = session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            ContentRange = contentRange,
            ChunkData = chunkData,
        };

        await SendRequestAsync(_egressRequestFactory.UploadChunkRequest(uploadArg, token));

        return new UploadChunkResult(TransferDirection.NetAppToEgress);
    }

    public async Task CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null)
    {
        var token = await GetWorkspaceToken();

        var completeArg = new CompleteUploadArg
        {
            UploadId = session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            WorkspaceId = session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            Md5Hash = md5hash,
        };

        await SendRequestAsync(_egressRequestFactory.CompleteUploadRequest(completeArg, token));
    }

    public async Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null)
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

    public async Task<DeleteFilesResult> DeleteFilesAsync(List<DeletionEntityDto> filesToDelete, string? workspaceId = null)
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

        try
        {
            var request = _egressRequestFactory.CreateFolderRequest(arg, token);

            var response = await _httpClient.SendAsync(request);

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
        // Get the last folder name from the base path (selected folder)
        var baseFolderName = Path.GetFileName(baseFolderPath.TrimEnd('/'));

        var baseFolderIndex = filePath.LastIndexOf(baseFolderName);

        if (baseFolderIndex >= 0)
        {
            var relativePath = filePath.Substring(baseFolderIndex);
            return Path.Combine(relativePath, fileName).Replace('\\', '/');
        }

        // Fallback: if we can't find the base folder in the path, use the original logic
        return Path.Combine(filePath, fileName).Replace('\\', '/');
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
}