using System.Threading.Tasks;
using System.Web.Http;
using Evonautinhas.API.Filters;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.API.Controllers
{
    [RoutePrefix("api/relatorios")]
    [ApiExceptionFilter]
    public class RelatoriosController : ApiController
    {
        private readonly IRelatorioService _relatorioService;

        public RelatoriosController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        [HttpGet]
        [Route("alunos-por-turma")]
        public async Task<IHttpActionResult> GetAlunosPorTurma()
        {
            return Ok(await _relatorioService.GetAlunosPorTurmaAsync());
        }
    }
}