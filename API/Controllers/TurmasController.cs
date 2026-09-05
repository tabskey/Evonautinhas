using System.Threading.Tasks;
using System.Web.Http;
using Evonautinhas.API.Filters;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.API.Controllers
{
    [RoutePrefix("api/turmas")]
    [ApiExceptionFilter]
    public class TurmasController : ApiController
    {
        private readonly ITurmaService _turmaService;

        public TurmasController(ITurmaService turmaService)
        {
            _turmaService = turmaService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            return Ok(await _turmaService.GetAllAsync());
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            return Ok(await _turmaService.GetByIdAsync(id));
        }
    }
}