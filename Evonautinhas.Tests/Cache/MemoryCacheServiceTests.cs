using System;
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
    }
}