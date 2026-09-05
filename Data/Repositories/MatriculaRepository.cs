using System.Threading.Tasks;
using Dapper;
using Evonautinhas.Data.Context;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Interfaces.Repositories;

namespace Evonautinhas.Data.Repositories
{
    public class MatriculaRepository : IMatriculaRepository
    {
        public MatriculaRepository(DatabaseContext databaseContext)
        {
        }

        public async Task<bool> ExistsAsync(int alunoId, int turmaId, System.Data.Common.DbTransaction transaction)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Matricula
                WHERE AlunoId = @AlunoId AND TurmaId = @TurmaId;";

            var count = await transaction.Connection.ExecuteScalarAsync<int>(
                sql,
                new { AlunoId = alunoId, TurmaId = turmaId },
                transaction);
            return count > 0;
        }

        public async Task<int> CreateAsync(Matricula matricula, System.Data.Common.DbTransaction transaction)
        {
            const string sql = @"
                INSERT INTO Matricula (AlunoId, TurmaId)
                OUTPUT INSERTED.Id
                VALUES (@AlunoId, @TurmaId);";

            return await transaction.Connection.ExecuteScalarAsync<int>(sql, matricula, transaction);
        }
    }
}