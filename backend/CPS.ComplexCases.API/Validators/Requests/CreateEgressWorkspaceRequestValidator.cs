using CPS.ComplexCases.API.Domain.Request;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class CreateEgressWorkspaceRequestValidator : AbstractValidator<CreateEgressWorkspaceRequest>
{
    public CreateEgressWorkspaceRequestValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty().WithMessage("CaseId is required.");
    }
}
