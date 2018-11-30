using System.Collections.Generic;
using System.Web.Http;
using InteractionPlanApi;
using InteractionPlanApi.App_Start;
using InteractionPlanApi.Extensions;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using NewVoiceMedia.AspNet.WebApi;
using NewVoiceMedia.AspNet.WebApi.Attributes;
using NewVoiceMedia.AspNet.WebApi.Extensions;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace InteractionPlanApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var kernel = NinjectWebCommon.CreateKernel();
            var config = new HttpConfiguration();
            AddVersioning(kernel, config);
            WebApiConfig.Register(config);
            config.AddCustomBindings();

            var scopes =
                new Dictionary<string, string>
                {
                    { "interaction-routing", "Route interactions" }
                };

            var requiredScopes =
                new[]
                {
                    "interaction-routing"
                };

            var supportedVersion = new List<int> { 1 };

            app
                .UseLog4Net()
                .UseSwaggerUi(typeof(Startup).Assembly, "/", scopes, supportedVersion)
                .UseOAuthAuthentication(requiredScopes)
                .UseNinjectMiddleware(() => kernel)
                .UseNinjectWebApi(config)
                .UseCors(CorsOptions.AllowAll);
        }

        private static void AddVersioning(IKernel kernel, HttpConfiguration config)
        {
            config.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new MediaTypeApiVersionReader(kernel.Get<NewVoiceMedia.CallCentre.Core.Logging.ILogger>());
                options.AssumeDefaultVersionWhenUnspecified = true;
            });
        }

        protected virtual IKernel CreateKernel()
        {
            return NinjectWebCommon.CreateKernel();
        }
    }
}