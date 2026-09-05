using System;
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
    }
}