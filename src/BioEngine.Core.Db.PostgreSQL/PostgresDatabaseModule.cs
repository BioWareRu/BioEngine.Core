using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace BioEngine.Core.Db.PostgreSQL
{
    public class PostgresDatabaseModule<TDbContext> : DatabaseModule<PostgresDatabaseModuleConfig>
        where TDbContext : DbContext
    {
        protected override void CheckConfig()
        {
            base.CheckConfig();

            if (string.IsNullOrEmpty(Config.Host))
            {
                throw new ArgumentException("Postgres host is empty");
            }

            if (string.IsNullOrEmpty(Config.Username))
            {
                throw new ArgumentException("Postgres username is empty");
            }

            if (string.IsNullOrEmpty(Config.Database))
            {
                throw new ArgumentException("Postgres database is empty");
            }

            if (Config.Port == 0)
            {
                throw new ArgumentException("Postgres host is empty");
            }
        }

        public override async Task InitAsync(IServiceProvider serviceProvider, IConfiguration configuration,
            IHostEnvironment environment)
        {
            await base.InitAsync(serviceProvider, configuration, environment);

            if (environment.IsProduction())
            {
                var dbContext = serviceProvider.GetRequiredService<TDbContext>();
                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    await dbContext.Database.MigrateAsync();
                }
            }
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = Config.Host,
                Port = Config.Port,
                Username = Config.Username,
                Password = Config.Password,
                Database = Config.Database,
                Pooling = Config.EnablePooling
            };

            Config.DbConfigure?.Invoke(connBuilder, configuration);
            services.AddEntityFrameworkNpgsql();
            services.AddDbContextPool<TDbContext>((p, options) =>
            {
                options.UseNpgsql(connBuilder.ConnectionString,
                    builder => builder.MigrationsAssembly(Config.MigrationsAssembly != null
                        ? Config.MigrationsAssembly.FullName
                        : typeof(DbContext).Assembly.FullName)).UseInternalServiceProvider(p);
                if (Config.EnableSensitiveLogging)
                {
                    options.EnableSensitiveDataLogging();
                }
            });
        }
    }

    public class PostgresDatabaseModuleConfig : DatabaseModuleConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public bool EnablePooling { get; set; }
        public bool EnableSensitiveLogging { get; set; }

        public Assembly? MigrationsAssembly
        {
            get;
        }

        public Action<NpgsqlConnectionStringBuilder, IConfiguration>? DbConfigure;

        public PostgresDatabaseModuleConfig(string host, string username, string database,
            string password = "",
            int port = 5432, bool enablePooling = true, Assembly migrationsAssembly = null)
        {
            Host = host;
            Username = username;
            Database = database;
            MigrationsAssembly = migrationsAssembly;
            EnablePooling = enablePooling;
            Password = password;
            Port = port;
        }
    }
}
