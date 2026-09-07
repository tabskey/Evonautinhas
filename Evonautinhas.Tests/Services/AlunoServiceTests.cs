using System;
using System.Threading.Tasks;
using Evonautinhas.Business.Services;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Exceptions;
using Evonautinhas.Domain.Interfaces.Repositories;
using Moq;
using NUnit.Framework;

namespace Evonautinhas.Tests.Services
{
    [TestFixture]
    public class AlunoServiceTests
    {
        [Test]
        public void CriarAlunoSemNome_DeveLancarValidationException()
        {
            var repository = new Mock<IAlunoRepository>();
            var service = new AlunoService(repository.Object);

            Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new Aluno
            {
                Email = "aluno@email.com",
                DataNascimento = new DateTime(2006, 3, 14, 0, 0, 0, DateTimeKind.Utc)
            }));
        }

        [Test]
        public void CriarAlunoSemDataNascimento_DeveLancarValidationException()
        {
            var repository = new Mock<IAlunoRepository>();
            var service = new AlunoService(repository.Object);

            Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new Aluno
            {
                Nome = "Ana Souza",
                Email = "ana@email.com"
            }));
        }

        [Test]
        public void CriarAlunoComEmailInvalido_DeveLancarValidationException()
        {
            var service = new AlunoService(new Mock<IAlunoRepository>().Object);

            Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new Aluno
            {
                Nome = "Ana Souza",
                Email = "email-sem-arroba",
                DataNascimento = new DateTime(2006, 3, 14)
            }));
        }

        [Test]
        public void CriarAlunoComNomeMuitoLongo_DeveLancarValidationException()
        {
            var service = new AlunoService(new Mock<IAlunoRepository>().Object);

            Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new Aluno
            {
                Nome = new string('A', 121),
                Email = "ana@email.com",
                DataNascimento = new DateTime(2006, 3, 14)
            }));
        }

        [Test]
        public void CriarAlunoComDataFutura_DeveLancarValidationException()
        {
            var service = new AlunoService(new Mock<IAlunoRepository>().Object);

            Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new Aluno
            {
                Nome = "Ana Souza",
                Email = "ana@email.com",
                DataNascimento = DateTime.Today.AddDays(1)
            }));
        }

        [Test]
        public async Task CriarAlunoValido_DeveDefinirAtivoTrueERetornarId()
        {
            var repository = new Mock<IAlunoRepository>();
            repository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Aluno)null);
            repository.Setup(x => x.CreateAsync(It.IsAny<Aluno>())).ReturnsAsync(42);

            var service = new AlunoService(repository.Object);
            var aluno = new Aluno
            {
                Nome = "Ana Souza",
                Email = "ana@email.com",
                DataNascimento = new DateTime(2006, 3, 14)
            };

            var id = await service.CreateAsync(aluno);

            Assert.That(id, Is.EqualTo(42));
            Assert.That(aluno.Ativo, Is.True);
            repository.Verify(x => x.CreateAsync(aluno), Times.Once);
        }

        [Test]
        public void CriarAlunoComEmailDeAlunoAtivo_DeveLancarBusinessRuleException()
        {
            var repository = new Mock<IAlunoRepository>();
            repository.Setup(x => x.GetByEmailAsync("ana@email.com"))
                .ReturnsAsync(new Aluno { Id = 1, Nome = "Ana Souza", Email = "ana@email.com", Ativo = true });

            var service = new AlunoService(repository.Object);

            Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(new Aluno
            {
                Nome = "Ana Souza",
                Email = "ana@email.com",
                DataNascimento = new DateTime(2006, 3, 14)
            }));

            repository.Verify(x => x.CreateAsync(It.IsAny<Aluno>()), Times.Never);
        }

        [Test]
        public void CriarAlunoComEmailDeAlunoArquivado_DeveLancarArchivedStudentException()
        {
            var repository = new Mock<IAlunoRepository>();
            repository.Setup(x => x.GetByEmailAsync("diego.ferreira@email.com"))
                .ReturnsAsync(new Aluno { Id = 4, Nome = "Diego Ferreira", Email = "diego.ferreira@email.com", Ativo = false });

            var service = new AlunoService(repository.Object);

            var ex = Assert.ThrowsAsync<ArchivedStudentException>(() => service.CreateAsync(new Aluno
            {
                Nome = "Diego Ferreira",
                Email = "diego.ferreira@email.com",
                DataNascimento = new DateTime(2005, 1, 30)
            }));

            Assert.That(ex.AlunoId, Is.EqualTo(4));
            repository.Verify(x => x.CreateAsync(It.IsAny<Aluno>()), Times.Never);
        }

        [Test]
        public async Task ReativarAluno_DeveChamarRepositorio()
        {
            var repository = new Mock<IAlunoRepository>();
            repository.Setup(x => x.ReactivateAsync(It.IsAny<Aluno>())).ReturnsAsync(true);

            var service = new AlunoService(repository.Object);
            var aluno = new Aluno
            {
                Id = 4,
                Nome = "Diego Ferreira Atualizado",
                Email = "diego.ferreira@email.com",
                DataNascimento = new DateTime(2005, 1, 30)
            };

            var resultado = await service.ReactivateAsync(aluno);

            Assert.That(resultado, Is.True);
            repository.Verify(x => x.ReactivateAsync(aluno), Times.Once);
        }

        [Test]
        public void BuscarAlunosComPaginaInvalida_DeveLancarValidationException()
        {
            var service = new AlunoService(new Mock<IAlunoRepository>().Object);

            Assert.Throws<ValidationException>(() => service.GetAllAsync(null, false, 0, 10));
        }

        [Test]
        public void BuscarAlunosComTamanhoAcimaDoMaximo_DeveLancarValidationException()
        {
            var service = new AlunoService(new Mock<IAlunoRepository>().Object);

            Assert.Throws<ValidationException>(() => service.GetAllAsync(null, false, 1, 101));
        }
    }
}
