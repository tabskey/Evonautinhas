using System.Data.SqlClient;
using System.Threading.Tasks;
using Evonautinhas.Data.Context;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Exceptions;
using Evonautinhas.Domain.Interfaces.Repositories;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.Business.Services
{
    public class MatriculaService : IMatriculaService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IAlunoRepository _alunoRepository;
        private readonly ITurmaRepository _turmaRepository;
        private readonly IMatriculaRepository _matriculaRepository;
        private readonly ITurmaService _turmaService;

        public MatriculaService(
            DatabaseContext databaseContext,
            IAlunoRepository alunoRepository,
            ITurmaRepository turmaRepository,
            IMatriculaRepository matriculaRepository,
            ITurmaService turmaService)
        {
            _databaseContext = databaseContext;
            _alunoRepository = alunoRepository;
            _turmaRepository = turmaRepository;
            _matriculaRepository = matriculaRepository;
            _turmaService = turmaService;
        }

        public async Task<int> CreateAsync(Matricula matricula)
        {
            ValidateRequest(matricula);
            await ValidarAlunoAtivo(matricula.AlunoId);
            await ValidarTurmaComVaga(matricula.TurmaId);

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (await _matriculaRepository.ExistsAsync(matricula.AlunoId, matricula.TurmaId, transaction))
                        {
                            throw new BusinessRuleException("Aluno já está matriculado nesta turma.");
                        }

                        var id = await _matriculaRepository.CreateAsync(matricula, transaction);

                        if (!await _turmaRepository.UpdateAvailableSpotsAsync(matricula.TurmaId, -1, transaction))
                        {
                            throw new BusinessRuleException("Turma sem vaga disponível.");
                        }

                        transaction.Commit();
                        _turmaService.InvalidarCache();
                        return id;
                    }
                    catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                    {
                        // Violação do UNIQUE (AlunoId, TurmaId) por corrida entre requests.
                        transaction.Rollback();
                        throw new BusinessRuleException("Aluno já está matriculado nesta turma.", ex);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private async Task ValidarAlunoAtivo(int alunoId)
        {
            var aluno = await _alunoRepository.GetByIdAsync(alunoId);
            if (aluno == null)
            {
                throw new NotFoundException("Aluno não encontrado.");
            }

            if (!aluno.Ativo)
            {
                throw new BusinessRuleException("Aluno está inativo.");
            }
        }

        private async Task ValidarTurmaComVaga(int turmaId)
        {
            var turma = await _turmaRepository.GetByIdAsync(turmaId);
            if (turma == null)
            {
                throw new NotFoundException("Turma não encontrada.");
            }

            if (turma.VagasDisponiveis <= 0)
            {
                throw new BusinessRuleException("Turma sem vaga disponível.");
            }
        }

        private static void ValidateRequest(Matricula matricula)
        {
            if (matricula == null || matricula.AlunoId <= 0 || matricula.TurmaId <= 0)
            {
                throw new ValidationException("AlunoId e TurmaId devem ser maiores que zero.");
            }
        }
    }
}