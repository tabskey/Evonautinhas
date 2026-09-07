using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Repositories
{
    public interface IAlunoRepository
    {
        Task<IEnumerable<Aluno>> GetAllAsync(string nome, bool incluirInativos, int offset, int pageSize);
        Task<Aluno> GetByIdAsync(int id);
        Task<Aluno> GetByEmailAsync(string email);
        Task<int> CountAsync(string nome, bool incluirInativos);
        Task<int> CreateAsync(Aluno aluno);
        Task<bool> UpdateAsync(Aluno aluno);
        Task<bool> ReactivateAsync(Aluno aluno);
        Task<bool> DeleteAsync(int id);
    }
}