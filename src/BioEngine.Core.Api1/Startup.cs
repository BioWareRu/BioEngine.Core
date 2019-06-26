using BioEngine.Core.Web;
using BioEngine.Core.Web.RenderService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace BioEngine.Core.Api
{
    public abstract class BioEngineApiStartup : BioEngineWebStartup
    {
        protected IHostEnvironment HostEnvironment { get; }

        protected BioEngineApiStartup(IConfiguration configuration, IHostEnvironment hostEnvironment) : base(
            configuration)
        {
            HostEnvironment = hostEnvironment;
        }

        protected override IMvcBuilder ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            return base.ConfigureMvc(mvcBuilder).AddApplicationPart(typeof(ResponseRestController<,,>).Assembly);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddScoped<IViewRenderService, ViewRenderService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = HostEnvironment.ApplicationName, Version = "v1"});
                //var security = new Dictionary<string, IEnumerable<string>> {,};

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Description = "Auth token",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.ApiKey
                        },
                        new string[] { }
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {Type = SecuritySchemeType.ApiKey});
            });
        }

        protected override void ConfigureAfterRouting(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", env.ApplicationName + " API V1"); });
        }
    }
}
