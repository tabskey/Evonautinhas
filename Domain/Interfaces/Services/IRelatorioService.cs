using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Services
{
    public interface IRelatorioService
    {
        Task<IEnumerable<RelatorioAlunosPorTurma>> GetAlunosPorTurmaAsync();
    }
}