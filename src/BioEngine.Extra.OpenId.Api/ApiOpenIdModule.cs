using System;
using BioEngine.Core.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.OpenId.Api
{
    public class
        ApiOpenIdModule<TUser, TUserPk, TUserDataProvider> : OpenIdModule<ApiOpenIdModuleConfig, TUser, TUserPk,
            TUserDataProvider>
        where TUser : IUser<TUserPk>, new() where TUserDataProvider : class, IUserDataProvider<TUserPk>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = Config.ServerUri.ToString();
                options.Audience = Config.Audience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
            });
        }
    }

    public class ApiOpenIdModuleConfig : OpenIdModuleConfig
    {
        public string Audience { get; }

        public ApiOpenIdModuleConfig(Uri serverUri, string audience) :
            base(serverUri)
        {
            Audience = audience;
        }
    }
}
