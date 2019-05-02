using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using UserManagement.Api.Constants;
using UserManagement.Api.Models.Responses;

namespace UserManagement.Api.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        [NonAction]
        public NotFoundObjectResult UserNotFound()
        {
            var errorResponse = new ErrorResponse(
                HttpContext.TraceIdentifier,
                ExternalErrorReason.ResourceNotFound,
                new[] { ExternalErrorReason.UserNotFound });

            return NotFound(errorResponse);
        }

        [NonAction]
        public ObjectResult GenericInvalidOperation(int statusCode, StringValues errorCodes)
        {
            var errorResponse = new ErrorResponse(
                HttpContext.TraceIdentifier,
                ExternalErrorReason.RequestInvalid,
                errorCodes);

            return new ObjectResult(errorResponse)
            {
                StatusCode = statusCode
            };
        }
    }
}