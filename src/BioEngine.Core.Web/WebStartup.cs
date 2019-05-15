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

        protected BioEngineWebStartup(IConfiguration configuration)
        {
            Configuration = configuration;
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

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (env.IsProduction())
            {
                var options = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };
                options.KnownProxies.Clear();
                options.KnownNetworks.Clear();
                options.RequireHeaderSymmetry = false;
                app.UseForwardedHeaders(options);
            }

            app.UseStaticFiles();

            ConfigureBeforeRouting(app, env);
            app.UseRouting();
            ConfigureAfterRouting(app, env);

            app.UseEndpoints(endpoints =>
            {
                ConfigureEndpoints(app, env, endpoints);
            });
        }
    }
}
