using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.DB
{
    public class InMemoryDatabaseModule<TDbContext> : DatabaseModule<InMemoryDatabaseModuleConfig> where TDbContext: DbContext
    {
        protected override void CheckConfig()
        {
            base.CheckConfig();
            if (string.IsNullOrEmpty(Config.InMemoryDatabaseName))
            {
                throw new ArgumentException("Empty inmemory database name");
            }
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.AddEntityFrameworkInMemoryDatabase();
            services.AddDbContext<TDbContext>((p, options) =>
            {
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .UseInMemoryDatabase(Config.InMemoryDatabaseName).UseInternalServiceProvider(p);
            });
        }
    }

    public class InMemoryDatabaseModuleConfig : DatabaseModuleConfig
    {
        public InMemoryDatabaseModuleConfig(string inMemoryDatabaseName)
        {
            InMemoryDatabaseName = inMemoryDatabaseName;
        }

        public string InMemoryDatabaseName { get; }
    }
}
