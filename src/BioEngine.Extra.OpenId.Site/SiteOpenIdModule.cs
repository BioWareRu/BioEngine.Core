using System;
using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.OpenId.Site
{
    public class
        SiteOpenIdModule<TUser, TUserPk, TUserDataProvider> : OpenIdModule<SiteOpenIdModuleConfig, TUser, TUserPk,
            TUserDataProvider>
        where TUser : IUser<TUserPk>, new() where TUserDataProvider : class, IUserDataProvider<TUserPk>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies", options =>
                {
                    options.ExpireTimeSpan = Config.CookieExpireTimeSpan;
                    options.SlidingExpiration = true;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = Config.ServerUri.ToString();
                    options.RequireHttpsMetadata = false;

                    options.ClientId = Config.ClientId;
                    options.ClientSecret = Config.ClientSecret;
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("offline_access");
                    if (Config.Scopes.Any())
                    {
                        foreach (var scope in Config.Scopes)
                        {
                            options.Scope.Add(scope);
                        }
                    }

                    options.ClaimActions.MapAll();
                }).AddAutomaticTokenManagement();
        }
    }

    public class SiteOpenIdModuleConfig : OpenIdModuleConfig
    {
        public string ClientId { get; }
        public string ClientSecret { get; }

        public TimeSpan CookieExpireTimeSpan { get; set; } = TimeSpan.FromDays(30);

        public IEnumerable<string> Scopes { get; } = new List<string>();

        public SiteOpenIdModuleConfig(Uri serverUri, string clientId, string clientSecret,
            IEnumerable<string>? scopes = null) : base(serverUri)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            if (scopes != null && scopes.Any())
            {
                Scopes = scopes;
            }
        }
    }
}
