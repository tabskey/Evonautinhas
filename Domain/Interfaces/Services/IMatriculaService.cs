using System.Threading.Tasks;
using Evonautinhas.Domain.Entities;

namespace Evonautinhas.Domain.Interfaces.Services
{
    public interface IMatriculaService
    {
        Task<int> CreateAsync(Matricula matricula);
    }
}