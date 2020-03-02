using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Web
{
    public abstract class BioEngineWebStartup
    {
        protected readonly IConfiguration Configuration;
        protected readonly IHostEnvironment Environment;

        protected BioEngineWebStartup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            ConfigureMvc(services.AddControllersWithViews())
                .AddNewtonsoftJson()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            ConfigureHealthChecks(services.AddHealthChecks());
            services.AddHttpContextAccessor();
        }

        protected virtual IMvcBuilder ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            return mvcBuilder;
        }

        protected virtual IHealthChecksBuilder ConfigureHealthChecks(IHealthChecksBuilder healthChecksBuilder)
        {
            return healthChecksBuilder;
        }

        protected virtual void ConfigureBeforeRouting(IApplicationBuilder app, IHostEnvironment env)
        {
        }

        protected virtual void ConfigureAfterRouting(IApplicationBuilder app, IHostEnvironment env)
        {
        }

        protected virtual void ConfigureEndpoints(IApplicationBuilder app, IHostEnvironment env,
            IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        }

        public virtual void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsProduction())
            {
                var options = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };
                options.KnownProxies.Clear();
                options.KnownNetworks.Clear();
                app.UseForwardedHeaders(options);
            }

            ConfigureStart(app);

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            ConfigureBeforeRouting(app, Environment);
            app.UseRouting();
            ConfigureAfterRouting(app, Environment);

            app.UseEndpoints(endpoints =>
            {
                ConfigureEndpoints(app, Environment, endpoints);
                RegisterEndpoints(endpoints);
            });
        }

        protected virtual void ConfigureStart(IApplicationBuilder appBuilder)
        {
        }
    }
}
