using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Evonautinhas.Data.Context;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Interfaces.Repositories;

namespace Evonautinhas.Data.Repositories
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly DatabaseContext _databaseContext;

        public RelatorioRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<IEnumerable<RelatorioAlunosPorTurma>> GetAlunosPorTurmaAsync()
        {
            const string sql = @"
                SELECT
                    t.Id AS TurmaId,
                    t.Nome AS TurmaNome,
                    t.Periodo,
                    t.VagasTotal,
                    t.VagasDisponiveis,
                    COUNT(m.Id) AS TotalAlunos
                FROM Turma AS t
                LEFT JOIN Matricula AS m ON m.TurmaId = t.Id
                GROUP BY t.Id, t.Nome, t.Periodo, t.VagasTotal, t.VagasDisponiveis
                ORDER BY t.Nome;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<RelatorioAlunosPorTurma>(sql);
            }
        }
    }
}