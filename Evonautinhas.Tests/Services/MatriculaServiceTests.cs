using Evonautinhas.Business.Services;
using Evonautinhas.Data.Context;
using Evonautinhas.Domain.Entities;
using Evonautinhas.Domain.Exceptions;
using Evonautinhas.Domain.Interfaces.Repositories;
using Evonautinhas.Domain.Interfaces.Services;
using Moq;
using NUnit.Framework;

namespace Evonautinhas.Tests.Services
{
    [TestFixture]
    public class MatriculaServiceTests
    {
        [Test]
        public void MatriculaSemVaga_DeveLancarBusinessRuleException()
        {
            var alunoRepository = new Mock<IAlunoRepository>();
            alunoRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Aluno { Id = 1, Ativo = true });

            var turmaRepository = new Mock<ITurmaRepository>();
            turmaRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Turma { Id = 1, VagasDisponiveis = 0 });

            var service = CreateService(alunoRepository, turmaRepository);

            Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(new Matricula
            {
                AlunoId = 1,
                TurmaId = 1
            }));
        }

        [Test]
        public void MatriculaAlunoInativo_DeveLancarBusinessRuleException()
        {
            var alunoRepository = new Mock<IAlunoRepository>();
            alunoRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Aluno { Id = 1, Ativo = false });

            var turmaRepository = new Mock<ITurmaRepository>();
            var service = CreateService(alunoRepository, turmaRepository);

            Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(new Matricula
            {
                AlunoId = 1,
                TurmaId = 1
            }));

            turmaRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void MatriculaComIdsInvalidos_DeveLancarValidationException()
        {
            var service = CreateService(new Mock<IAlunoRepository>(), new Mock<ITurmaRepository>());

            Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new Matricula
            {
                AlunoId = 0,
                TurmaId = 1
            }));
        }

        private static MatriculaService CreateService(
            Mock<IAlunoRepository> alunoRepository,
            Mock<ITurmaRepository> turmaRepository)
        {
            return new MatriculaService(
                new DatabaseContext("Server=unused;Database=unused;Trusted_Connection=True;"),
                alunoRepository.Object,
                turmaRepository.Object,
                new Mock<IMatriculaRepository>().Object,
                new Mock<ITurmaService>().Object);
        }
    }
}