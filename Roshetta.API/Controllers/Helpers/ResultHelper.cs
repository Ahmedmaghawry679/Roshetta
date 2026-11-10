using Microsoft.AspNetCore.Mvc;
using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Controllers.Helpers
{
    public static class ResultHelper
    {
        public static ActionResult ToActionResult(this Result result) 
            => result.IsSuccess? new OkResult():MapError(result.ErrorType, result.Errors);

        // Result<T> (with data)
        public static ActionResult ToActionResult<T>(this Result<T> result)
            => result.IsSuccess? new OkObjectResult(result.Data) : MapError(result.ErrorType, result.Errors);

        private static ObjectResult MapError(ErrorType? errorType, IEnumerable<string> errors)
        {
            var (status, title) = errorType switch
            {
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
                ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title
            };
            problem.Extensions["errors"] = errors;

            return new ObjectResult(problem) { StatusCode = status };
        }
    }
}
