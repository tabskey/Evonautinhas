using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Interfaces.Repositories;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.Business.Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly IRelatorioRepository _relatorioRepository;

        public RelatorioService(IRelatorioRepository relatorioRepository)
        {
            _relatorioRepository = relatorioRepository;
        }

        public Task<IEnumerable<RelatorioAlunosPorTurma>> GetAlunosPorTurmaAsync()
        {
            return _relatorioRepository.GetAlunosPorTurmaAsync();
        }
    }
}