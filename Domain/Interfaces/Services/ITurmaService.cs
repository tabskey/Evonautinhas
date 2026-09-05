using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Services
{
    public interface ITurmaService
    {
        Task<IEnumerable<Turma>> GetAllAsync();
        Task<Turma> GetByIdAsync(int id);
        void InvalidarCache();
    }
}