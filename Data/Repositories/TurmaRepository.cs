using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Evonautinhas.Data.Context;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Interfaces.Repositories;

namespace Evonautinhas.Data.Repositories
{
    public class TurmaRepository : ITurmaRepository
    {
        private readonly DatabaseContext _databaseContext;

        public TurmaRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<IEnumerable<Turma>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, Nome, Periodo, VagasTotal, VagasDisponiveis
                FROM Turma
                ORDER BY Nome;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<Turma>(sql);
            }
        }

        public async Task<Turma> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Nome, Periodo, VagasTotal, VagasDisponiveis
                FROM Turma
                WHERE Id = @Id;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<Turma>(sql, new { Id = id });
            }
        }

        public async Task<bool> UpdateAvailableSpotsAsync(int id, int delta, System.Data.Common.DbTransaction transaction)
        {
            const string sql = @"
                UPDATE Turma
                SET VagasDisponiveis = VagasDisponiveis + @Delta
                WHERE Id = @Id AND VagasDisponiveis + @Delta >= 0;";

            return await transaction.Connection.ExecuteAsync(sql, new { Id = id, Delta = delta }, transaction) > 0;
        }
    }
}