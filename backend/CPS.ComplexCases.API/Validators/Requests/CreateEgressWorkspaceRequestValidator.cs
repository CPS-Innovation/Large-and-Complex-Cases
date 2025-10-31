using CPS.ComplexCases.API.Domain.Request;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class CreateEgressWorkspaceRequestValidator : AbstractValidator<CreateEgressWorkspaceRequest>
{
    public CreateEgressWorkspaceRequestValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty().WithMessage("CaseId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage("TemplateId is required.");
    }
}