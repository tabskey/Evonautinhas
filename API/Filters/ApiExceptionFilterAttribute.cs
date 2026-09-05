using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Evonautinhas.Domain.Exceptions;

namespace Evonautinhas.API.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var status = HttpStatusCode.InternalServerError;
            if (context.Exception is ValidationException)
            {
                status = HttpStatusCode.BadRequest;
            }
            else if (context.Exception is NotFoundException)
            {
                status = HttpStatusCode.NotFound;
            }
            else if (context.Exception is BusinessRuleException)
            {
                status = HttpStatusCode.Conflict;
            }

            context.Response = context.Request.CreateResponse(status, new
            {
                message = context.Exception.Message
            });
        }
    }
}