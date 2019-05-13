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
        public string Url { get; set; } = "";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
