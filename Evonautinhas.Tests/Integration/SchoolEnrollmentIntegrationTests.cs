using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Evonautinhas.Business.Cache;
using Evonautinhas.Business.Services;
using Evonautinhas.Data.Context;
using Evonautinhas.Data.Repositories;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Exceptions;
using NUnit.Framework;

namespace Evonautinhas.Tests.Integration
{
    /// <summary>
    /// Suíte de integração contra um SQL Server LocalDB dedicado (Evonautinhas_Tests).
    /// O banco é recriado a cada teste a partir do script canônico Database/001-create-schema.sql
    /// (copiado para o output via csproj), garantindo estado determinístico e fonte única do schema.
    /// Rode somente onde houver LocalDB; caso contrário a suíte é ignorada, não falha.
    /// </summary>
    internal static class TestDatabase
    {
        private const string Server = @"(localdb)\MSSQLLocalDB";
        private const string Name = "Evonautinhas_Tests";

        public static string MasterConnectionString =>
            $@"Data Source={Server};Initial Catalog=master;Integrated Security=True;";

        public static string ConnectionString =>
            $@"Data Source={Server};Initial Catalog={Name};Integrated Security=True;";

        public static void EnsureAvailable()
        {
            try
            {
                using (var connection = new SqlConnection(MasterConnectionString))
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                Assert.Ignore("LocalDB indisponível. Inicie com 'sqllocaldb create MSSQLLocalDB -s'. Detalhe: " + ex.Message);
            }
        }

        public static async Task RecreateAsync()
        {
            SqlConnection.ClearAllPools();
            using (var master = new SqlConnection(MasterConnectionString))
            {
                await master.OpenAsync();
                using (var command = master.CreateCommand())
                {
                    command.CommandText = $@"
                        IF DB_ID('{Name}') IS NOT NULL
                        BEGIN
                            ALTER DATABASE [{Name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [{Name}];
                        END;
                        CREATE DATABASE [{Name}];";
                    await command.ExecuteNonQueryAsync();
                }
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                foreach (var batch in ReadBatchesFromSchemaScript())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = batch;
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        private static IEnumerable<string> ReadBatchesFromSchemaScript()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "001-create-schema.sql");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Script 001-create-schema.sql não encontrado no output dos testes.",
                    path);
            }

            var buffer = new List<string>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    var batch = string.Join("\n", buffer).Trim();
                    buffer.Clear();
                    if (ShouldExecute(batch))
                    {
                        yield return batch;
                    }

                    continue;
                }

                buffer.Add(line);
            }

            var tail = string.Join("\n", buffer).Trim();
            if (ShouldExecute(tail))
            {
                yield return tail;
            }
        }

        private static bool ShouldExecute(string batch)
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                return false;
            }

            // O script cria/USE a base "TesteEscola"; aqui o banco já foi criado com outro nome.
            if (batch.IndexOf("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            if (batch.TrimStart().StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }

    [TestFixture]
    [Category("Integration")]
    public class MatriculaFlowIntegrationTests
    {
        private DatabaseContext _context;
        private AlunoRepository _alunoRepository;
        private TurmaService _turmaService;
        private MatriculaService _matriculaService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestDatabase.EnsureAvailable();
        }

        [SetUp]
        public async Task SetUp()
        {
            await TestDatabase.RecreateAsync();

            _context = new DatabaseContext(TestDatabase.ConnectionString);
            _alunoRepository = new AlunoRepository(_context);
            var turmaRepository = new TurmaRepository(_context);
            var matriculaRepository = new MatriculaRepository(_context);

            var cache = new MemoryCacheService();
            cache.Clear();
            _turmaService = new TurmaService(turmaRepository, cache);
            _matriculaService = new MatriculaService(
                _context,
                _alunoRepository,
                turmaRepository,
                matriculaRepository,
                _turmaService);
        }

        [Test]
        public async Task MatricularAlunoEmTurmaComVaga_DeveInserirDecrementarVagaEInvalidarCache()
        {
            // Seed: turma 2 (30/30) e aluno 1 sem vínculo com ela.
            var antes = await _turmaService.GetAllAsync(); // popula o cache
            Assert.That(antes.Single(t => t.Id == 2).VagasDisponiveis, Is.EqualTo(30));

            var id = await _matriculaService.CreateAsync(new Matricula { AlunoId = 1, TurmaId = 2 });

            Assert.That(id, Is.GreaterThan(0));
            Assert.That(await CountMatriculasAsync(1, 2), Is.EqualTo(1));

            var turma = await _turmaService.GetByIdAsync(2);
            Assert.That(turma.VagasDisponiveis, Is.EqualTo(29));

            // Segunda leitura via cache: só retorna 29 se a invalidação pós-commit funcionou
            // (o TTL é de 5 minutos, então um cache velho ainda estaria em 30).
            var depois = await _turmaService.GetAllAsync();
            Assert.That(depois.Single(t => t.Id == 2).VagasDisponiveis, Is.EqualTo(29));
        }

        [Test]
        public void MatricularAlunoJaMatriculado_DeveLancarBusinessRuleException()
        {
            // Seed: aluno 1 já está na turma 1.
            var ex = Assert.ThrowsAsync<BusinessRuleException>(() =>
                _matriculaService.CreateAsync(new Matricula { AlunoId = 1, TurmaId = 1 }));

            StringAssert.Contains("já está matriculado", ex.Message);
        }

        [Test]
        public async Task InsercaoDiretaDuplicada_DeveViolarConstraintUnique()
        {
            // Bypass do service para provar que a garantia de unicidade vive no banco.
            using (var connection = _context.CreateConnection())
            {
                await connection.OpenAsync();
                var ex = Assert.ThrowsAsync<SqlException>(() =>
                    connection.ExecuteAsync(
                        "INSERT INTO Matricula (AlunoId, TurmaId) VALUES (@AlunoId, @TurmaId);",
                        new { AlunoId = 1, TurmaId = 1 }));

                Assert.That(ex.Number, Is.EqualTo(2601).Or.EqualTo(2627));
            }
        }

        [Test]
        public async Task MatricularAlunoInativo_DeveLancarBusinessRuleExceptionSemAlterarNada()
        {
            // Seed: aluno 4 (Diego) está inativo.
            Assert.ThrowsAsync<BusinessRuleException>(() =>
                _matriculaService.CreateAsync(new Matricula { AlunoId = 4, TurmaId = 2 }));

            Assert.That(await CountMatriculasAsync(4, 2), Is.EqualTo(0));
            var turma = await _turmaService.GetByIdAsync(2);
            Assert.That(turma.VagasDisponiveis, Is.EqualTo(30), "vaga não pode ser descontada");
        }

        [Test]
        public void MatricularAlunoOuTurmaInexistente_DeveLancarNotFoundException()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                _matriculaService.CreateAsync(new Matricula { AlunoId = 9999, TurmaId = 2 }));

            Assert.ThrowsAsync<NotFoundException>(() =>
                _matriculaService.CreateAsync(new Matricula { AlunoId = 1, TurmaId = 9999 }));
        }

        [Test]
        public async Task ConcorrenciaComUmaUnicaVaga_DeveMatricularApenasUmAluno()
        {
            // Seed: turma 3 (Intensiva) tem 1 vaga restante. Insere 6 alunos novos ativos.
            var novosAlunos = new List<int>();
            for (var i = 0; i < 6; i++)
            {
                var id = await _alunoRepository.CreateAsync(new Aluno
                {
                    Nome = "Aluno Concorrencia " + i,
                    Email = "concorrencia" + i + "@teste.com",
                    DataNascimento = new DateTime(2006, 1, 1),
                    Ativo = true
                });
                novosAlunos.Add(id);
            }

            // Todos disputam a única vaga da turma 3 em paralelo.
            var resultados = await Task.WhenAll(novosAlunos.Select(id => TryMatricularAsync(id, 3)));

            var sucessos = resultados.Count(ok => ok);
            Assert.That(sucessos, Is.EqualTo(1), "exatamente um aluno deve conseguir a vaga");

            using (var connection = _context.CreateConnection())
            {
                await connection.OpenAsync();
                var totalNaTurma = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM Matricula WHERE TurmaId = 3;");
                var vagas = await connection.ExecuteScalarAsync<int>(
                    "SELECT VagasDisponiveis FROM Turma WHERE Id = 3;");

                // 4 alunos do seed + 1 vencedor; vaga zera sem ficar negativa.
                Assert.That(totalNaTurma, Is.EqualTo(5));
                Assert.That(vagas, Is.EqualTo(0));
            }
        }

        private async Task<bool> TryMatricularAsync(int alunoId, int turmaId)
        {
            try
            {
                await _matriculaService.CreateAsync(new Matricula { AlunoId = alunoId, TurmaId = turmaId });
                return true;
            }
            catch (BusinessRuleException)
            {
                return false; // perdeu a corrida (pré-checagem ou guarda atômica)
            }
        }

        private async Task<int> CountMatriculasAsync(int alunoId, int turmaId)
        {
            using (var connection = _context.CreateConnection())
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM Matricula WHERE AlunoId = @AlunoId AND TurmaId = @TurmaId;",
                    new { AlunoId = alunoId, TurmaId = turmaId });
            }
        }
    }

    [TestFixture]
    [Category("Integration")]
    public class AlunoPersistenceIntegrationTests
    {
        private AlunoRepository _alunoRepository;
        private AlunoService _alunoService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestDatabase.EnsureAvailable();
        }

        [SetUp]
        public async Task SetUp()
        {
            await TestDatabase.RecreateAsync();

            var context = new DatabaseContext(TestDatabase.ConnectionString);
            _alunoRepository = new AlunoRepository(context);
            _alunoService = new AlunoService(_alunoRepository);
        }

        [Test]
        public async Task AtualizarAluno_NaoDeveAlterarAtivoNoBanco()
        {
            // Regressão do bug em que o PUT gravava Ativo = false (arquivava sem querer).
            var aluno = await _alunoRepository.GetByIdAsync(1);
            Assert.That(aluno.Ativo, Is.True, "pré-condição: seed ativo");

            var atualizado = await _alunoRepository.UpdateAsync(new Aluno
            {
                Id = 1,
                Nome = "Ana Souza Editada",
                Email = "ana.souza@email.com",
                DataNascimento = new DateTime(2006, 3, 14)
                // Ativo fica no default false de propósito: não pode sobrescrever.
            });
            Assert.That(atualizado, Is.True);

            var persistido = await _alunoRepository.GetByIdAsync(1);
            Assert.That(persistido.Nome, Is.EqualTo("Ana Souza Editada"));
            Assert.That(persistido.Ativo, Is.True, "Ativo não pode ser alterado por um update comum");
        }

        [Test]
        public async Task CriarAlunoValido_PersisteAtivoTrueERetornaId()
        {
            var id = await _alunoService.CreateAsync(new Aluno
            {
                Nome = "Aluno Novo",
                Email = "novo@teste.com",
                DataNascimento = new DateTime(2007, 5, 10)
            });

            var persistido = await _alunoRepository.GetByIdAsync(id);
            Assert.That(persistido, Is.Not.Null);
            Assert.That(persistido.Ativo, Is.True);
        }

        [Test]
        public void CriarAlunoComEmailDuplicado_DeveLancarBusinessRuleException()
        {
            // Seed já contém ana.souza@email.com.
            Assert.ThrowsAsync<BusinessRuleException>(() =>
                _alunoService.CreateAsync(new Aluno
                {
                    Nome = "Outra Ana",
                    Email = "ana.souza@email.com",
                    DataNascimento = new DateTime(2006, 3, 14)
                }));
        }

        [Test]
        public async Task CriarAlunoComEmailDeArquivado_DeveSinalizarReativacao()
        {
            // Diego (id 4) está inativo no seed.
            var ex = Assert.ThrowsAsync<ArchivedStudentException>(() =>
                _alunoService.CreateAsync(new Aluno
                {
                    Nome = "Diego Ferreira",
                    Email = "diego.ferreira@email.com",
                    DataNascimento = new DateTime(2005, 1, 30)
                }));

            Assert.That(ex.AlunoId, Is.EqualTo(4));
            StringAssert.Contains("arquivado", ex.Message);
        }

        [Test]
        public async Task ReativarAlunoArquivado_DeveAtualizarDadosEVoltarAtivo()
        {
            var reativado = await _alunoService.ReactivateAsync(new Aluno
            {
                Id = 4,
                Nome = "Diego Ferreira (Reinscrito)",
                Email = "diego.ferreira@email.com",
                DataNascimento = new DateTime(2005, 1, 30)
            });
            Assert.That(reativado, Is.True);

            var persistido = await _alunoRepository.GetByIdAsync(4);
            Assert.That(persistido.Ativo, Is.True);
            Assert.That(persistido.Nome, Is.EqualTo("Diego Ferreira (Reinscrito)"));

            // Ativo de novo: um novo cadastro com o mesmo e-mail vira 409 comum (duplicado).
            Assert.ThrowsAsync<BusinessRuleException>(() =>
                _alunoService.CreateAsync(new Aluno
                {
                    Nome = "Diego de novo",
                    Email = "diego.ferreira@email.com",
                    DataNascimento = new DateTime(2005, 1, 30)
                }));
        }

        [Test]
        public async Task ReativarAlunoInexistente_DeveRetornarFalse()
        {
            var resultado = await _alunoService.ReactivateAsync(new Aluno
            {
                Id = 9999,
                Nome = "Ninguém",
                Email = "ninguem@teste.com",
                DataNascimento = new DateTime(2005, 1, 1)
            });

            Assert.That(resultado, Is.False);
        }
    }
}
