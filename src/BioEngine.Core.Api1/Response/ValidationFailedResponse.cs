using System.Collections.Generic;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Api.Response
{
    public class ValidationFailedResponse : ObjectResult
    {
        public ValidationFailedResponse(IEnumerable<ValidationFailure> errors)
            : base(new ValidationResponse(errors))
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity;
        }
    }
}