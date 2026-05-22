using CPS.ComplexCases.Common.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.FileTransfer.API.Validators;

public class ProvisionNetAppFoldersRequestValidator : AbstractValidator<ProvisionNetAppFoldersRequest>
{
    public ProvisionNetAppFoldersRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be greater than 0.");

        RuleFor(x => x.Urn)
            .NotEmpty()
            .WithMessage("Urn is required.");

        RuleFor(x => x.TemplateName)
            .NotEmpty()
            .WithMessage("TemplateName is required.");

        RuleFor(x => x.BucketName)
            .NotEmpty()
            .WithMessage("BucketName is required.");

        RuleFor(x => x.BearerToken)
            .NotEmpty()
            .WithMessage("BearerToken is required.");
    }
}