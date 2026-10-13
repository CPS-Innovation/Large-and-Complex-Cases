namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;


public class GovNotifyOptions
{
    public const string SectionName = "GovNotifyOptions";

    /// <summary>
    /// GOV.UK Notify API key. When empty, notifications are skipped.
    /// </summary>
    public string? ApiKey { get; set; }

    public string TransferCompletedTemplateId { get; set; } = "d0a988ee-2d1b-4e41-8ef4-444728fc4c44";

    public string TransferFailedTemplateId { get; set; } = "96e58eea-8484-4372-9426-c178aa505c74";

    public string UiBaseUrl { get; set; } = string.Empty;
}
