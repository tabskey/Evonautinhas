using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace Evonautinhas.API.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.DependencyResolver = new DependencyResolver();

            // API com contrato JSON em camelCase, como documentado.
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}