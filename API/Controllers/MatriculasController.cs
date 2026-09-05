using System.Threading.Tasks;
using System.Web.Http;
using Evonautinhas.API.Filters;
using Evonautinhas.API.Models;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.API.Controllers
{
    [RoutePrefix("api/matriculas")]
    [ApiExceptionFilter]
    public class MatriculasController : ApiController
    {
        private readonly IMatriculaService _matriculaService;

        public MatriculasController(IMatriculaService matriculaService)
        {
            _matriculaService = matriculaService;
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create(MatriculaRequest request)
        {
            var id = await _matriculaService.CreateAsync(new Matricula
            {
                AlunoId = request == null ? 0 : request.AlunoId,
                TurmaId = request == null ? 0 : request.TurmaId
            });
            return Created("api/matriculas/" + id, new { id });
        }
    }
}