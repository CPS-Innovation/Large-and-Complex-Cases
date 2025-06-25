using CPS.ComplexCases.Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace CPS.ComplexCases.Common.Helpers;

public interface IRequestValidator
{
    Task<ValidatableRequest<T>> GetJsonBody<T, V>(HttpRequest request)
      where V : AbstractValidator<T>, new();
}