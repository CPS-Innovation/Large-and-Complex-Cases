using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;
using CPS.ComplexCases.Common.Models.Domain.Dtos;

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

    public async Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string? workspaceId = null, string? sourcePath = null, TransferOverwritePolicy? overwritePolicy = null)
    {
        var token = await GetWorkspaceToken();

        if (string.IsNullOrEmpty(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath), "Source path cannot be null or empty.");

        var fileName = Path.GetFileName(sourcePath);

        if (overwritePolicy != TransferOverwritePolicy.Overwrite)
        {
            // check to see if filename exists in the destination path
            // egress does not have endpoint to get file from path so we have to list all in the path and check if filename exists
            var listArg = new ListWorkspaceMaterialArg
            {
                WorkspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
                Path = destinationPath,
            };
            var listResponse = await SendRequestAsync<ListCaseMaterialResponse>(_egressRequestFactory.ListEgressMaterialRequest(listArg, token));

            if (listResponse.Data.Any(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new FileExistsException($"File '{fileName}' already exists in the destination path '{destinationPath}'.");
            }
        }

        var arg = new CreateUploadArg
        {
            FolderPath = destinationPath,
            FileSize = fileSize,
            WorkspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            FileName = fileName ?? throw new ArgumentNullException(nameof(sourcePath), "sourcePath path cannot be null."),
        };

        var response = await SendRequestAsync<CreateUploadResponse>(_egressRequestFactory.CreateUploadRequest(arg, token));

        return new UploadSession
        {
            UploadId = response.Id,
            WorkspaceId = workspaceId,
            Md5Hash = response.Md5Hash,
        };
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
                    SourcePath = entity.Path
                }
                };
            }
            else
            {
                return await GetAllFilesFromFolderParallel(workspaceId, entity.Id, token);
            }
        });

        var results = await Task.WhenAll(entityTasks);
        return results.SelectMany(files => files);
    }

    private async Task<List<FileTransferInfo>> GetAllFilesFromFolderParallel(string workspaceId, string? folderId, string token)
    {
        var allPagesData = await GetAllPagesInParallel(workspaceId, folderId, token);

        var files = allPagesData
            .Where(d => !d.IsFolder)
            .Select(d => new FileTransferInfo
            {
                Id = d.Id,
                SourcePath = Path.Combine(d.Path, d.FileName).Replace('\\', '/')
            })
            .ToList();

        var folders = allPagesData.Where(d => d.IsFolder).ToList();

        if (folders.Any())
        {
            var subFolderTasks = folders.Select(folder =>
                GetAllFilesFromFolderParallel(workspaceId, folder.Id, token));

            var subFolderResults = await Task.WhenAll(subFolderTasks);
            files.AddRange(subFolderResults.SelectMany(x => x));
        }

        return files;
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
                    RecurseSubFolders = false
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