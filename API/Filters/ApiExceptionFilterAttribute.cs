using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Evonautinhas.Domain.Exceptions;

namespace Evonautinhas.API.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is ArchivedStudentException archived)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.Conflict, new
                {
                    message = archived.Message,
                    errorCode = "ALUNO_ARQUIVADO",
                    alunoId = archived.AlunoId
                });
                return;
            }

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

            var message = context.Exception.Message;
            if (status == HttpStatusCode.InternalServerError && !IsDebuggingEnabled())
            {
                // Não vazar detalhes internos (mensagens SQL, stack traces) para o cliente.
                message = "Ocorreu um erro inesperado.";
            }

            context.Response = context.Request.CreateResponse(status, new
            {
                message
            });
        }

        private static bool IsDebuggingEnabled()
        {
            return HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled;
        }
    }
}
