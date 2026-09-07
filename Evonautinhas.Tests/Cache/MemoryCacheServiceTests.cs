using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Evonautinhas.Business.Cache;
using NUnit.Framework;

namespace Evonautinhas.Tests.Cache
{
    [TestFixture]
    public class MemoryCacheServiceTests
    {
        [Test]
        public async Task GetOrSetAsync_ComMesmoId_DeveReutilizarValorEmCache()
        {
            var cache = new MemoryCacheService();
            var chamadas = 0;

            var primeiro = await cache.GetOrSetAsync(
                "teste-cache-reutilizacao",
                () =>
                {
                    chamadas++;
                    return Task.FromResult("valor");
                },
                TimeSpan.FromMinutes(1));

            var segundo = await cache.GetOrSetAsync(
                "teste-cache-reutilizacao",
                () =>
                {
                    chamadas++;
                    return Task.FromResult("outro-valor");
                },
                TimeSpan.FromMinutes(1));

            Assert.That(primeiro, Is.EqualTo("valor"));
            Assert.That(segundo, Is.EqualTo("valor"));
            Assert.That(chamadas, Is.EqualTo(1));
            cache.Remove("teste-cache-reutilizacao");
        }

        [Test]
        public async Task Remove_DeveForcarNovaCarga()
        {
            var cache = new MemoryCacheService();
            var chamadas = 0;

            await cache.GetOrSetAsync(
                "teste-cache-invalidacao",
                () =>
                {
                    chamadas++;
                    return Task.FromResult(chamadas);
                },
                TimeSpan.FromMinutes(1));

            cache.Remove("teste-cache-invalidacao");
            var valor = await cache.GetOrSetAsync(
                "teste-cache-invalidacao",
                () =>
                {
                    chamadas++;
                    return Task.FromResult(chamadas);
                },
                TimeSpan.FromMinutes(1));

            Assert.That(valor, Is.EqualTo(2));
            Assert.That(chamadas, Is.EqualTo(2));
            cache.Remove("teste-cache-invalidacao");
        }

        [Test]
        public async Task GetOrSetAsync_ComAcessosConcorrentes_DeveExecutarFactoryUmaUnicaVez()
        {
            var cache = new MemoryCacheService();
            var chamadas = 0;

            var tarefas = Enumerable.Range(0, 8).Select(_ => cache.GetOrSetAsync(
                "teste-cache-concorrente",
                () =>
                {
                    Interlocked.Increment(ref chamadas);
                    Thread.Sleep(150); // mantém a janela de corrida aberta
                    return Task.FromResult(1);
                },
                TimeSpan.FromMinutes(1)));

            var resultados = await Task.WhenAll(tarefas);

            Assert.That(chamadas, Is.EqualTo(1), "a factory deve executar apenas uma vez sob concorrência");
            Assert.That(resultados, Is.All.EqualTo(1));
            cache.Remove("teste-cache-concorrente");
        }
    }
}
