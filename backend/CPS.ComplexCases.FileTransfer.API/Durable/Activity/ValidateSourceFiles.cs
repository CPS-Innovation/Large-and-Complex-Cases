using System.Net;
using Amazon.S3;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

/// <summary>
/// Lightweight source existence check run before transfer fan-out. Missing files are returned
/// rather than thrown so the orchestrator can poll briefly, then fail them as SourceFileNotFound.
/// Access/config failures are returned as Failed so they are not polled. 5xx is rethrown so a
/// single Durable retry of this activity remains possible.
/// </summary>
public class ValidateSourceFiles(
    IStorageClientFactory storageClientFactory,
    IInitializationHandler initializationHandler,
    ILogger<ValidateSourceFiles> logger)
{
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ILogger<ValidateSourceFiles> _logger = logger;

    [Function(nameof(ValidateSourceFiles))]
    public async Task<ValidateSourceFilesResult> Run([ActivityTrigger] ValidateSourceFilesPayload payload)
    {
        _initializationHandler.Initialize(payload.UserName!, payload.CorrelationId, payload.CaseId);

        var result = new ValidateSourceFilesResult();
        if (payload.SourcePaths.Count == 0)
        {
            return result;
        }

        var sourceClient = _storageClientFactory.GetClientsForDirection(payload.TransferDirection).source;

        foreach (var sourcePath in payload.SourcePaths)
        {
            try
            {
                var exists = await sourceClient.FileExistsAsync(
                    sourcePath.Path,
                    payload.WorkspaceId,
                    payload.BearerToken,
                    payload.BucketName,
                    sourcePath.FileId);

                if (exists)
                {
                    result.Available.Add(sourcePath);
                }
                else
                {
                    result.Missing.Add(sourcePath);
                }
            }
            catch (Exception ex) when (IsNotFound(ex))
            {
                result.Missing.Add(sourcePath);
            }
            catch (Exception ex) when (IsServerError(ex))
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Source file access failed during pre-flight validation for {Path}",
                    sourcePath.Path);

                result.Failed.Add(new TransferFailedItem
                {
                    SourcePath = sourcePath.Path,
                    Status = TransferItemStatus.Failed,
                    ErrorCode = TransferErrorCode.GeneralError,
                    ErrorMessage = TransferErrorMessages.GetUserMessage(TransferErrorCode.GeneralError)
                });
            }
        }

        return result;
    }

    internal static bool IsNotFound(Exception ex) =>
        ex is FileNotFoundException
        || (ex is HttpRequestException http && http.StatusCode == HttpStatusCode.NotFound)
        || (ex is AmazonS3Exception s3 && s3.StatusCode == HttpStatusCode.NotFound);

    internal static bool IsServerError(Exception ex) =>
        (ex is HttpRequestException http && (int?)http.StatusCode >= 500)
        || (ex is AmazonS3Exception s3 && (int)s3.StatusCode >= 500);
}
