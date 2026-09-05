using System;
using System.Threading.Tasks;
using System.Web.Http;
using Evonautinhas.API.Filters;
using Evonautinhas.API.Models;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.API.Controllers
{
    [RoutePrefix("api/alunos")]
    [ApiExceptionFilter]
    public class AlunosController : ApiController
    {
        private readonly IAlunoService _alunoService;

        public AlunosController(IAlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll(string nome = null, bool incluirInativos = false, int pagina = 1, int tamanho = 10)
        {
            var itens = await _alunoService.GetAllAsync(nome, incluirInativos, pagina, tamanho);
            var total = await _alunoService.CountAsync(nome, incluirInativos);
            return Ok(new
            {
                total,
                pagina,
                tamanho,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanho),
                itens
            });
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var aluno = await _alunoService.GetByIdAsync(id);
            return aluno == null ? (IHttpActionResult)NotFound() : Ok(aluno);
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create(AlunoRequest request)
        {
            var id = await _alunoService.CreateAsync(ToEntity(request));
            return Created("api/alunos/" + id, new { id });
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Update(int id, AlunoRequest request)
        {
            var aluno = ToEntity(request);
            aluno.Id = id;
            if (!await _alunoService.UpdateAsync(aluno))
            {
                return NotFound();
            }

            return Ok(aluno);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            return await _alunoService.DeleteAsync(id) ? (IHttpActionResult)StatusCode(System.Net.HttpStatusCode.NoContent) : NotFound();
        }

        private static Aluno ToEntity(AlunoRequest request)
        {
            if (request == null)
            {
                return null;
            }

            return new Aluno
            {
                Nome = request.Nome,
                Email = request.Email,
                DataNascimento = request.DataNascimento
            };
        }
    }
}