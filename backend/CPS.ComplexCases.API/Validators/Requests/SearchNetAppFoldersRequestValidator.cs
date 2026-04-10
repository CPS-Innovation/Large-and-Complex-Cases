using CPS.ComplexCases.NetApp.Models.Requests;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class SearchNetAppFoldersRequestValidator : AbstractValidator<SearchNetAppFoldersDto>
{
    public SearchNetAppFoldersRequestValidator()
    {
        RuleFor(x => x.CaseId).GreaterThan(0).WithMessage("CaseId must be provided.");
        RuleFor(x => x.Query).NotEmpty().WithMessage("Query must be provided.");
        RuleFor(x => x.MaxResults).InclusiveBetween(1, 1000).WithMessage("MaxResults must be between 1 and 1000.");
    }
}