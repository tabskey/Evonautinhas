using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Evonautinhas.API.Controllers;
using Evonautinhas.Business.Cache;
using Evonautinhas.Business.Services;
using Evonautinhas.Data.Context;
using Evonautinhas.Data.Repositories;
using Evonautinhas.Domain.Interfaces.Repositories;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.API.App_Start
{

    public sealed class DependencyResolver : IDependencyResolver
    {
        private readonly ICacheService _cacheService;

        public DependencyResolver()
        {
            _cacheService = new MemoryCacheService();
        }

        public IDependencyScope BeginScope()
        {
            return new DependencyScope(_cacheService);
        }

        public object GetService(Type serviceType)
        {
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new object[0];
        }

        public void Dispose()
        {

        }
    }

    internal sealed class DependencyScope : IDependencyScope
    {
        private readonly IAlunoService _alunoService;
        private readonly ITurmaService _turmaService;
        private readonly IMatriculaService _matriculaService;
        private readonly IRelatorioService _relatorioService;

        private bool _disposed;

        public DependencyScope(ICacheService cacheService)
        {
            var databaseContext = new DatabaseContext();
            var alunoRepository = new AlunoRepository(databaseContext);
            var turmaRepository = new TurmaRepository(databaseContext);
            var matriculaRepository = new MatriculaRepository(databaseContext);
            var relatorioRepository = new RelatorioRepository(databaseContext);

            _alunoService = new AlunoService(alunoRepository);
            _turmaService = new TurmaService(turmaRepository, cacheService);
            _matriculaService = new MatriculaService(
                databaseContext,
                alunoRepository,
                turmaRepository,
                matriculaRepository,
                _turmaService);
            _relatorioService = new RelatorioService(relatorioRepository);
        }

        public IDependencyScope BeginScope()
        {
            throw new NotSupportedException("Este container não suporta escopos aninhados.");
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(AlunosController))
                return new AlunosController(_alunoService);

            if (serviceType == typeof(TurmasController))
                return new TurmasController(_turmaService);

            if (serviceType == typeof(MatriculasController))
                return new MatriculasController(_matriculaService);

            if (serviceType == typeof(RelatoriosController))
                return new RelatoriosController(_relatorioService);

            if (serviceType == typeof(IAlunoService))
                return _alunoService;

            if (serviceType == typeof(ITurmaService))
                return _turmaService;

            if (serviceType == typeof(IMatriculaService))
                return _matriculaService;

            if (serviceType == typeof(IRelatorioService))
                return _relatorioService;

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new object[0];
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

     
        }
    }
}
