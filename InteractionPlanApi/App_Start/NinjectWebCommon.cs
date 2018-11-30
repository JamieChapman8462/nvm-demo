using System.Web.Http;
using System.Web.Http.Filters;
using InteractionPlanApi.Auth;
using InteractionPlanApi.Request.Base.Interaction;
using InteractionPlanApi.Request.NamedRoute;
using InteractionPlanApi.Service;
using CallRecordingService.DependencyInjection;
using InteractionPlanApi.Request;
using NewVoiceMedia.CallCentre.CallPlanRuntime.Service.DependencyInjection;
using NewVoiceMedia.CallCentre.Core.DependencyInjection;
using NewVoiceMedia.CallCentre.Core.Logging;
using NewVoiceMedia.CallCentre.InteractionPlan;
using NewVoiceMedia.CallCentre.Model.DependencyInjection;
using NewVoiceMedia.Common;
using NewVoiceMedia.Common.Infrastructure.DataAccess;
using NewVoiceMedia.Ninject;
using NewVoiceMedia.Services.DependencyInjection;
using NewVoiceMedia.Statistics.Business.DependencyInjection;
using ILogger = NewVoiceMedia.CallCentre.Core.Logging.ILogger;

namespace InteractionPlanApi.App_Start
{
    using System;
    using System.Web;

    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.WebApi.FilterBindingSyntax;
    using NewVoiceMedia.CallCentre.ToggleService.DependencyInjection;

    public static class NinjectWebCommon
    {
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        internal static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                GlobalConfiguration.Configuration.DependencyResolver = new NinjectResolver(kernel);

                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load<CoreModule>();
            kernel.Load<ModelModule>();
            kernel.Load<WebSettingsModule>();
            kernel.Load<ServicesModule>();
            kernel.Load<CallPlanRuntimeModule>();
            kernel.Load<StatisticsBusinessModule>();
            kernel.Load<ToggleServiceModule>();
            kernel.Load<CallRecordingServiceWriteableModule>();
            kernel.Load<InteractionPlanCoreModule>();

            kernel.Bind<ILogger>().ToMethod(l => kernel.Get<ILoggerFactory>().GetLogger()).InSingletonScope();

            kernel.Bind<IDbAccessConfiguration>().To<DbAccessConfig>();

            kernel.Bind<IHttpRequest>().ToMethod(ctx => new NewVoiceMedia.Common.HttpRequestWrapper(() => HttpContext.Current.Request)).InRequestScope();

            kernel.Bind<IInteractionBaseMapper>().To<InteractionBaseMapper>();
            kernel.Bind<IInvocationMapper>().To<InvocationMapper>();
            kernel.Bind<IInvokeRouteService>().To<InvokeRouteService>();
            kernel.Bind<IInvokeRouteServiceMetricRecorder>().To<InvokeRouteServiceMetricRecorder>();
            kernel.Bind<IApiProviderRegistrationService>().To<ApiProviderRegistrationService>();
            kernel.Bind<IInteractionRoutingService>().To<InteractionRoutingService>();

            kernel.BindHttpFilter<KongAuthenticationFilter>(FilterScope.Controller)
                .WhenControllerHas<KongAuthenticationAttribute>()
                .WithConstructorArgumentFromControllerAttribute<KongAuthenticationAttribute>("requiredScope", a => a.RequiredScope);

            kernel.BindHttpFilter<AccountAuthenticationFilter>(FilterScope.Controller)
                .WhenControllerHas<AccountAuthenticationAttribute>();

            kernel.BindHttpFilter<FeatureEnabledAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<RequireFeatureEnabledAttribute>()
                .WithConstructorArgumentFromControllerAttribute<RequireFeatureEnabledAttribute>("requiredFeature", a => a.RequiredFeatureType);

            kernel.Bind<NccoQueue>().ToSelf().InSingletonScope();
        }
    }
}
