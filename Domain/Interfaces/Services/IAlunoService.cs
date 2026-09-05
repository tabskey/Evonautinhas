using System.Collections.Generic;
using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Services
{
    public interface IAlunoService
    {
        Task<IEnumerable<Aluno>> GetAllAsync(string nome, bool incluirInativos, int pagina, int tamanho);
        Task<int> CountAsync(string nome, bool incluirInativos);
        Task<Aluno> GetByIdAsync(int id);
        Task<int> CreateAsync(Aluno aluno);
        Task<bool> UpdateAsync(Aluno aluno);
        Task<bool> DeleteAsync(int id);
    }
}