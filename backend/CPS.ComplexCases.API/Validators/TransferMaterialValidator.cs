

using CPS.ComplexCases.API.Domain;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators;

public class TransferMaterialValidator : AbstractValidator<TransferMaterialDto>
{
  public TransferMaterialValidator()
  {
    RuleFor(x => x.DestinationPath).NotEmpty().WithMessage("Destination path is required.");
    RuleFor(x => x.FilePaths).NotEmpty().WithMessage("At least one file path is required.");
  }
}
