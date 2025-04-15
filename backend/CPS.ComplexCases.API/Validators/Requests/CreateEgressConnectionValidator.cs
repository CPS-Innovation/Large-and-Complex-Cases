using CPS.ComplexCases.Data.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class CreateEgressConnectionValidator : AbstractValidator<CreateEgressConnectionDto>
{
  public CreateEgressConnectionValidator()
  {
    RuleFor(x => x.CaseId).NotEmpty().WithMessage("CaseId is required.");
    RuleFor(x => x.EgressWorkspaceId).NotEmpty().WithMessage("EgressWorkspaceId is required.");
  }
}