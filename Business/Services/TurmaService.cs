using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Exceptions;
using Evonautinhas.Domain.Interfaces.Repositories;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.Business.Services
{
    public class TurmaService : ITurmaService
    {
        private const string CacheKey = "turmas:todas";
        private readonly ITurmaRepository _turmaRepository;
        private readonly ICacheService _cacheService;

        public TurmaService(ITurmaRepository turmaRepository, ICacheService cacheService)
        {
            _turmaRepository = turmaRepository;
            _cacheService = cacheService;
        }

        public Task<IEnumerable<Turma>> GetAllAsync()
        {
            return _cacheService.GetOrSetAsync(
                CacheKey,
                () => _turmaRepository.GetAllAsync(),
                TimeSpan.FromMinutes(5));
        }

        public async Task<Turma> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id da turma deve ser maior que zero.");
            }

            var turma = await _turmaRepository.GetByIdAsync(id);
            if (turma == null)
            {
                throw new NotFoundException("Turma não encontrada.");
            }

            return turma;
        }

        public void InvalidarCache()
        {
            _cacheService.Remove(CacheKey);
        }
    }
}