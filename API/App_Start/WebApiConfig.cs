using System.Web.Http;
using Evonautinhas.API.Controllers;
using Evonautinhas.Business.Cache;
using Evonautinhas.Business.Services;
using Evonautinhas.Data.Context;
using Evonautinhas.Data.Repositories;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.API.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.DependencyResolver = new DependencyResolver();
        }
    }
}