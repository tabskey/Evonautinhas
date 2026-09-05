using System.Data.Common;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Repositories
{
    public interface IMatriculaRepository
    {
        Task<bool> ExistsAsync(int alunoId, int turmaId, DbTransaction transaction);
        Task<int> CreateAsync(Matricula matricula, DbTransaction transaction);
    }
}