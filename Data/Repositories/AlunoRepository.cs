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

        public async Task<IEnumerable<Aluno>> GetAllAsync(string nome, bool incluirInativos, int offset, int pageSize)
        {
            const string sql = @"
                                SELECT
                                        a.Id,
                                        a.Nome,
                                        a.Email,
                                        a.DataNascimento,
                                        a.Ativo,
                                        a.DataCadastro,
                                        STUFF((
                                                SELECT ', ' + t.Nome
                                                FROM Matricula AS m
                                                INNER JOIN Turma AS t ON t.Id = m.TurmaId
                                                WHERE m.AlunoId = a.Id
                                                FOR XML PATH(''), TYPE
                                        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Turmas
                                FROM Aluno AS a
                                WHERE (@Nome IS NULL OR a.Nome LIKE '%' + @Nome + '%' ESCAPE '\')
                                    AND (@IncluirInativos = 1 OR a.Ativo = 1)
                                ORDER BY a.Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<Aluno>(sql, new
                {
                    Nome = EscapeLikePattern(nome),
                    IncluirInativos = incluirInativos,
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

        public async Task<Aluno> GetByEmailAsync(string email)
        {
            const string sql = @"
                SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                FROM Aluno
                WHERE Email = @Email;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<Aluno>(sql, new { Email = email });
            }
        }

        public async Task<int> CountAsync(string nome, bool incluirInativos)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Aluno
                WHERE (@Nome IS NULL OR Nome LIKE '%' + @Nome + '%' ESCAPE '\')
                  AND (@IncluirInativos = 1 OR Ativo = 1);";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<int>(sql, new
                {
                    Nome = EscapeLikePattern(nome)
                    , IncluirInativos = incluirInativos
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
            // Ativo NÃO é alterado aqui: arquivamento (soft delete) só acontece via DeleteAsync.
            const string sql = @"
                UPDATE Aluno
                SET Nome = @Nome, Email = @Email, DataNascimento = @DataNascimento
                WHERE Id = @Id;";

            using (var connection = _databaseContext.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sql, aluno) > 0;
            }
        }

        public async Task<bool> ReactivateAsync(Aluno aluno)
        {
            // Reinscrição de aluno arquivado: atualiza os dados informados e volta Ativo = 1.
            const string sql = @"
                UPDATE Aluno
                SET Nome = @Nome, Email = @Email, DataNascimento = @DataNascimento, Ativo = 1
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

        private static string EscapeLikePattern(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return null;
            }

            // Escapa curingas do LIKE para que % e _ sejam tratados como texto literal.
            return term
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }
    }
}
