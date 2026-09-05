using System.Web;
using System.Web.Http;
using Evonautinhas.API.App_Start;

namespace Evonautinhas
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}