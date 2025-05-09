using CPS.ComplexCases.Data.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class CreateNetAppConnectionValidator : AbstractValidator<CreateNetAppConnectionDto>
{
    public CreateNetAppConnectionValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty().WithMessage("CaseId is required.");
        RuleFor(x => x.OperationName).NotEmpty().WithMessage("BucketName is required.");
        RuleFor(x => x.NetAppFolderPath).NotEmpty().WithMessage("NetAppFolderPath is required.");
    }
}