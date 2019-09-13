using System.Collections.Generic;
using System.Security.Claims;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Users
{
    public abstract class
        BaseUsersModule<TConfig, TUser, TUserDataProvider, TCurrentUserProvider> : BaseBioEngineModule<TConfig>
        where TConfig : BaseUsersModuleConfig
        where TUser : IUser
        where TUserDataProvider : class, IUserDataProvider
        where TCurrentUserProvider : class, ICurrentUserProvider
    {
        public override void ConfigureEntities(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager)
        {
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services.AddScoped<IUserDataProvider, TUserDataProvider>();
            services.AddScoped<ICurrentUserProvider, TCurrentUserProvider>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(BioPolicies.Admin,
                    builder => builder.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "admin"));
                options.AddPolicy(BioPolicies.User, builder => builder.RequireAuthenticatedUser());
                foreach (var policy in Config.Policies)
                {
                    options.AddPolicy(policy.Key, policy.Value);
                }
            });
        }
    }

    public abstract class BaseUsersModuleConfig
    {
        public Dictionary<string, AuthorizationPolicy> Policies { get; } =
            new Dictionary<string, AuthorizationPolicy>();
    }
}
