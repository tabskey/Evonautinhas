using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Repositories
{
    public interface ITurmaRepository
    {
        Task<IEnumerable<Turma>> GetAllAsync();
        Task<Turma> GetByIdAsync(int id);
        Task<bool> UpdateAvailableSpotsAsync(int id, int delta, DbTransaction transaction);
    }
}