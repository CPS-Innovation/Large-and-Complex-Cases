using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CPS.ComplexCases.API.Validators;

public static class ValidatorHelper
{
  public static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(HttpRequest request)
      where V : AbstractValidator<T>, new()
  {
    using var reader = new StreamReader(request.Body);
    var requestJson = await reader.ReadToEndAsync();

    T? requestObject;
    try
    {
      requestObject = JsonSerializer.Deserialize<T>(requestJson);
    }
    catch (JsonException)
    {
      return InvalidRequest<T>("Invalid JSON format.");
    }

    if (requestObject == null)
    {
      return InvalidRequest<T>("Deserialized object is null.");
    }

    var validator = new V();
    var validationResult = await validator.ValidateAsync(requestObject);

    return new ValidatableRequest<T>
    {
      Value = requestObject,
      IsValid = validationResult.IsValid,
      ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
    };
  }

  private static ValidatableRequest<T> InvalidRequest<T>(string errorMessage)
  {
    return new ValidatableRequest<T>
    {
      Value = default!,
      IsValid = false,
      ValidationErrors = new List<string> { errorMessage }
    };
  }
}