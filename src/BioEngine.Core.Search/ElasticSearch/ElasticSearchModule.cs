using System;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Search.ElasticSearch
{
    public class ElasticSearchModule : SearchModule<ElasticSearchModuleConfig>
    {
        protected override void CheckConfig()
        {
            if (string.IsNullOrEmpty(Config.Url))
            {
                throw new ArgumentException("Elastic url is empty");
            }
        }

        protected override void ConfigureSearch(IServiceCollection services)
        {
            services.AddScoped<ISearcher, ElasticSearcher>();
            services.AddSingleton(Config);
        }
    }

    public class ElasticSearchModuleConfig : SearchModuleConfig
    {
        public ElasticSearchModuleConfig(string prefix, string url, string login = "", string password = "", bool enableClientLogging = false)
        {
            Prefix = prefix;
            Url = url;
            Login = login;
            Password = password;
            EnableClientLogging = enableClientLogging;
        }

        public string Prefix { get; }
        public string Url { get; }
        public string Login { get; }
        public string Password { get; }
        public bool EnableClientLogging { get; }
    }
}
