using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Evonautinhas.Data.Context;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Interfaces.Repositories;

namespace Evonautinhas.Data.Repositories
{
    public class AlunoRepository : IAlunoRepository
    {
        private readonly DatabaseContext _databaseContext;

        public AlunoRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<IEnumerable<Aluno>> GetAllAsync(string nome, int offset, int pageSize)
        {
            const string sql = @"
                SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                FROM Aluno
                WHERE (@Nome IS NULL OR Nome LIKE '%' + @Nome + '%')
                  AND Ativo = 1
                ORDER BY Nome
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<Aluno>(sql, new
                {
                    Nome = string.IsNullOrWhiteSpace(nome) ? null : nome,
                    Offset = offset,
                    PageSize = pageSize
                });
            }
        }

        public async Task<Aluno> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                FROM Aluno
                WHERE Id = @Id;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<Aluno>(sql, new { Id = id });
            }
        }

        public async Task<int> CountAsync(string nome)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Aluno
                WHERE (@Nome IS NULL OR Nome LIKE '%' + @Nome + '%')
                  AND Ativo = 1;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<int>(sql, new
                {
                    Nome = string.IsNullOrWhiteSpace(nome) ? null : nome
                });
            }
        }

        public async Task<int> CreateAsync(Aluno aluno)
        {
            const string sql = @"
                INSERT INTO Aluno (Nome, Email, DataNascimento, Ativo)
                OUTPUT INSERTED.Id
                VALUES (@Nome, @Email, @DataNascimento, @Ativo);";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<int>(sql, aluno);
            }
        }

        public async Task<bool> UpdateAsync(Aluno aluno)
        {
            const string sql = @"
                UPDATE Aluno
                SET Nome = @Nome, Email = @Email, DataNascimento = @DataNascimento, Ativo = @Ativo
                WHERE Id = @Id;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sql, aluno) > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"
                UPDATE Aluno
                SET Ativo = 0
                WHERE Id = @Id;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
            }
        }
    }
}