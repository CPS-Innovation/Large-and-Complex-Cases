using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using FluentValidation;

namespace CPS.ComplexCases.API.Validators.Requests;

public class InitiateTransferRequestValidator : AbstractValidator<InitiateTransferRequest>
{
    public InitiateTransferRequestValidator()
    {
        RuleFor(x => x.DestinationPath).NotEmpty().WithMessage("DestinationPath is required.");
        RuleFor(x => x.TransferType).IsInEnum().WithMessage("TransferType must be either Copy or Move.");
        RuleFor(x => x.TransferDirection).IsInEnum().WithMessage("TransferDirection must be either EgressToNetApp or NetAppToEgress.");
        RuleFor(x => x.SourcePaths).Must(x => x.Count > 0).WithMessage("At least one SourcePath is required.");
        RuleFor(x => x.CaseId).NotEmpty().WithMessage("CaseId is required.");
        RuleFor(x => x.WorkspaceId).NotEmpty().WithMessage("WorkspaceId is required.");

        RuleFor(x => x)
            .Custom((request, context) =>
            {
                if (request.TransferDirection == TransferDirection.NetAppToEgress && request.TransferType != TransferType.Copy)
                {
                    context.AddFailure("TransferType", "When TransferDirection is NetAppToEgress, TransferType must be Copy.");
                }
                else if (request.TransferDirection == TransferDirection.EgressToNetApp &&
                         request.TransferType != TransferType.Copy && request.TransferType != TransferType.Move)
                {
                    context.AddFailure("TransferType", "When TransferDirection is EgressToNetApp, TransferType must be Copy or Move.");
                }
            });
    }
}