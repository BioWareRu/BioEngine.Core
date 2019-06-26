using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Extensions;
using FluentValidation.Results;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace BioEngine.Core.Api.Response
{
    public class ValidationResponse : RestResponse
    {
        [UsedImplicitly] public string Message { get; } = "Success";

        [UsedImplicitly] public bool IsSuccess { get; } = true;

        public ValidationResponse(IEnumerable<ValidationFailure> errors) : base(StatusCodes
            .Status422UnprocessableEntity)
        {
            var validationFailures = errors as ValidationFailure[] ?? errors.ToArray();
            if (validationFailures.Any())
            {
                IsSuccess = false;
                Message = "Validation Failed";
                Errors = validationFailures
                    .Select(error => new ValidationErrorResponse(error.PropertyName.ToCamelCase(), error.ErrorMessage))
                    .ToList();
            }
        }
    }
}