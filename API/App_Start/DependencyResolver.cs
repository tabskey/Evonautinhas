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
    public class DependencyResolver : IDependencyResolver
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IAlunoRepository _alunoRepository;
        private readonly ITurmaRepository _turmaRepository;
        private readonly IMatriculaRepository _matriculaRepository;
        private readonly ICacheService _cacheService;

        public DependencyResolver()
        {
            _databaseContext = new DatabaseContext();
            _alunoRepository = new AlunoRepository(_databaseContext);
            _turmaRepository = new TurmaRepository(_databaseContext);
            _matriculaRepository = new MatriculaRepository(_databaseContext);
            _cacheService = new MemoryCacheService();
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(AlunosController))
            {
                return new AlunosController(new AlunoService(_alunoRepository));
            }

            if (serviceType == typeof(TurmasController))
            {
                return new TurmasController(new TurmaService(_turmaRepository, _cacheService));
            }

            if (serviceType == typeof(MatriculasController))
            {
                return new MatriculasController(CreateMatriculaService());
            }

            if (serviceType == typeof(RelatoriosController))
            {
                return new RelatoriosController(new RelatorioService(new RelatorioRepository(_databaseContext)));
            }

            if (serviceType == typeof(IAlunoService))
            {
                return new AlunoService(_alunoRepository);
            }

            if (serviceType == typeof(ITurmaService))
            {
                return new TurmaService(_turmaRepository, _cacheService);
            }

            if (serviceType == typeof(IMatriculaService))
            {
                return CreateMatriculaService();
            }

            return null;
        }

        private IMatriculaService CreateMatriculaService()
        {
            return new MatriculaService(
                _databaseContext,
                _alunoRepository,
                _turmaRepository,
                _matriculaRepository,
                new TurmaService(_turmaRepository, _cacheService));
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new object[0];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}