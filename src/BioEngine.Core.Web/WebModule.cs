using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Web
{
    public class WebModule<T> : BioEngineModule<T> where T : WebModuleConfig
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<BaseControllerContext>();
            services.AddScoped(typeof(BaseControllerContext<,>));
        }
    }

    public abstract class WebModuleConfig
    {
    }
}
