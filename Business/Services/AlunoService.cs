using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Exceptions;
using Evonautinhas.Domain.Interfaces.Repositories;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.Business.Services
{
    public class AlunoService : IAlunoService
    {
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
            aluno.Ativo = true;
            return await _alunoRepository.CreateAsync(aluno);
        }

        public async Task<bool> UpdateAsync(Aluno aluno)
        {
            ValidateAluno(aluno);
            ValidateId(aluno.Id);
            return await _alunoRepository.UpdateAsync(aluno);
        }

        public Task<bool> DeleteAsync(int id)
        {
            ValidateId(id);
            return _alunoRepository.DeleteAsync(id);
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

            if (string.IsNullOrWhiteSpace(aluno.Email))
            {
                throw new ValidationException("E-mail do aluno é obrigatório.");
            }

            if (aluno.DataNascimento == DateTime.MinValue)
            {
                throw new ValidationException("Data de nascimento do aluno é obrigatória.");
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