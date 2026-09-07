using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Exceptions;
using Evonautinhas.Domain.Interfaces.Repositories;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.Business.Services
{
    public class AlunoService : IAlunoService
    {
        private const int NomeMaxLength = 120;   // espelha NVARCHAR(120) do banco
        private const int EmailMaxLength = 120;
        private const int PageSizeMax = 100;

        private static readonly Regex EmailPattern =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private readonly IAlunoRepository _alunoRepository;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
        }

        public Task<IEnumerable<Aluno>> GetAllAsync(string nome, bool incluirInativos, int pagina, int tamanho)
        {
            if (pagina < 1 || tamanho < 1)
            {
                throw new ValidationException("Página e tamanho devem ser maiores que zero.");
            }

            if (tamanho > PageSizeMax)
            {
                throw new ValidationException("Tamanho máximo por página é " + PageSizeMax + ".");
            }

            return _alunoRepository.GetAllAsync(nome, incluirInativos, (pagina - 1) * tamanho, tamanho);
        }

        public Task<Aluno> GetByIdAsync(int id)
        {
            ValidateId(id);
            return _alunoRepository.GetByIdAsync(id);
        }

        public Task<int> CountAsync(string nome, bool incluirInativos)
        {
            return _alunoRepository.CountAsync(nome, incluirInativos);
        }

        public async Task<int> CreateAsync(Aluno aluno)
        {
            ValidateAluno(aluno);

            // Se o e-mail pertence a um cadastro já existente, decide o caminho:
            // ativo → duplicidade real (409); arquivado → oferece reativação.
            var existing = await _alunoRepository.GetByEmailAsync(aluno.Email);
            if (existing != null)
            {
                if (existing.Ativo)
                {
                    throw new BusinessRuleException("Já existe um aluno cadastrado com este e-mail.");
                }

                throw new ArchivedStudentException(
                    existing.Id,
                    "Já existe um aluno arquivado com este e-mail (" + existing.Nome + ").");
            }

            aluno.Ativo = true;
            try
            {
                return await _alunoRepository.CreateAsync(aluno);
            }
            catch (SqlException ex) when (IsUniqueViolation(ex))
            {
                throw new BusinessRuleException("Já existe um aluno cadastrado com este e-mail.", ex);
            }
        }

        public async Task<bool> UpdateAsync(Aluno aluno)
        {
            ValidateAluno(aluno);
            ValidateId(aluno.Id);
            try
            {
                return await _alunoRepository.UpdateAsync(aluno);
            }
            catch (SqlException ex) when (IsUniqueViolation(ex))
            {
                throw new BusinessRuleException("Já existe um aluno cadastrado com este e-mail.", ex);
            }
        }

        public async Task<bool> ReactivateAsync(Aluno aluno)
        {
            ValidateAluno(aluno);
            ValidateId(aluno.Id);
            try
            {
                return await _alunoRepository.ReactivateAsync(aluno);
            }
            catch (SqlException ex) when (IsUniqueViolation(ex))
            {
                throw new BusinessRuleException("Já existe um aluno cadastrado com este e-mail.", ex);
            }
        }

        public Task<bool> DeleteAsync(int id)
        {
            ValidateId(id);
            return _alunoRepository.DeleteAsync(id);
        }

        private static bool IsUniqueViolation(SqlException ex)
        {
            return ex.Number == 2601 || ex.Number == 2627;
        }

        private static void ValidateAluno(Aluno aluno)
        {
            if (aluno == null)
            {
                throw new ValidationException("Aluno é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(aluno.Nome))
            {
                throw new ValidationException("Nome do aluno é obrigatório.");
            }

            if (aluno.Nome.Length > NomeMaxLength)
            {
                throw new ValidationException("Nome do aluno não pode exceder " + NomeMaxLength + " caracteres.");
            }

            if (string.IsNullOrWhiteSpace(aluno.Email))
            {
                throw new ValidationException("E-mail do aluno é obrigatório.");
            }

            if (aluno.Email.Length > EmailMaxLength)
            {
                throw new ValidationException("E-mail do aluno não pode exceder " + EmailMaxLength + " caracteres.");
            }

            if (!EmailPattern.IsMatch(aluno.Email))
            {
                throw new ValidationException("E-mail do aluno é inválido.");
            }

            if (aluno.DataNascimento == DateTime.MinValue)
            {
                throw new ValidationException("Data de nascimento do aluno é obrigatória.");
            }

            if (aluno.DataNascimento > DateTime.Today)
            {
                throw new ValidationException("Data de nascimento do aluno não pode ser no futuro.");
            }
        }

        private static void ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id do aluno deve ser maior que zero.");
            }
        }
    }
}
