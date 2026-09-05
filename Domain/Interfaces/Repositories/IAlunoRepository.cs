using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Repositories
{
    public interface IAlunoRepository
    {
        Task<IEnumerable<Aluno>> GetAllAsync(string nome, int offset, int pageSize);
        Task<Aluno> GetByIdAsync(int id);
        Task<int> CountAsync(string nome);
        Task<int> CreateAsync(Aluno aluno);
        Task<bool> UpdateAsync(Aluno aluno);
        Task<bool> DeleteAsync(int id);
    }
}