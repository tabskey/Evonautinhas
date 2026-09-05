using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Repositories
{
    public interface IRelatorioRepository
    {
        Task<IEnumerable<RelatorioAlunosPorTurma>> GetAlunosPorTurmaAsync();
    }
}