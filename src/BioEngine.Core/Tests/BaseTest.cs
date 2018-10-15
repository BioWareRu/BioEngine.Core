using System;
using System.Collections.Generic;
using BioEngine.Core.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public abstract class BaseTest
    {
        protected ITestOutputHelper TestOutputHelper { get; }

        protected BaseTest(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }
    }

    public abstract class BaseTest<T> : BaseTest, IDisposable where T : BaseTestScope
    {
        protected BaseTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        private readonly Dictionary<string, BaseTestScope> _scopes = new Dictionary<string, BaseTestScope>();

        private BaseTestScope GetScope(string name)
        {
            if (!_scopes.ContainsKey(name))
            {
                var scope = Activator.CreateInstance<T>();
                scope.Configure(name, TestOutputHelper);
                _scopes.Add(name, scope);
            }

            return _scopes[name];
        }

        protected BioContext GetDbContext(string name, bool init = true)
        {
            return GetScope(name).GetDbContext(init);
        }

        public void Dispose()
        {
            foreach (var testScope in _scopes)
            {
                testScope.Value.Dispose();
            }
        }
    }

    public abstract class BaseTestScope : IDisposable
    {
        public void Configure(string dbName, ITestOutputHelper testOutputHelper)
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets("bw")
                .AddEnvironmentVariables()
                .Build();
            var services = new ServiceCollection();
            services.AddLogging(o => o.AddProvider(new XunitLoggerProvider(testOutputHelper)));
            ConfigureServices(services);
            bool.TryParse(_configuration["BE_TESTS_POSTGRES"], out var testWithPostgres);
            if (testWithPostgres)
            {
                services.AddEntityFrameworkNpgsql();
            }
            else
            {
                services.AddEntityFrameworkInMemoryDatabase();
            }

            services.AddDbContext<BioContext>((p, optionsBuilder) =>
            {
                optionsBuilder.EnableSensitiveDataLogging().UseInternalServiceProvider(p);

                if (testWithPostgres)
                {
                    optionsBuilder
                        .UseNpgsql(GetConnectionString(dbName))
                        .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
                }
                else
                {
                    optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                        .UseInMemoryDatabase(dbName);
                }

                ModifyDbOptions(optionsBuilder.Options, !testWithPostgres);
            });
            _serviceProvider = services.BuildServiceProvider();
        }

        protected virtual IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services;
        }

        private BioContext _bioContext;

        public BioContext GetDbContext(bool needInit)
        {
            if (_bioContext == null)
            {
                _bioContext = _serviceProvider.GetService<BioContext>();
                if (_bioContext == null)
                {
                    throw new Exception("Can't create db context");
                }

                _bioContext.Database.EnsureDeleted();
                _bioContext.Database.EnsureCreated();
                if (needInit)
                {
                    InitDbContext(_bioContext);
                }
            }

            return _bioContext;
        }

        protected virtual string GetConnectionString(string databaseName)
        {
            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = _configuration["BIO_POSTGRES_HOST"] ?? "localhost",
                Port =
                    !string.IsNullOrEmpty(_configuration["BIO_POSTGRES_PORT"])
                        ? int.Parse(_configuration["BIO_POSTGRES_PORT"])
                        : 5432,
                Username = _configuration["BIO_POSTGRES_USERNAME"] ?? "postgres",
                Password = _configuration["BIO_POSTGRES_PASSWORD"] ?? "",
                Database = databaseName,
                Pooling = false
            };
            return connBuilder.ConnectionString;
        }

        private IConfiguration _configuration;
        private IServiceProvider _serviceProvider;

        protected virtual void InitDbContext(BioContext dbContext)
        {
        }

        protected virtual void ModifyDbOptions(DbContextOptions options, bool inMemory)
        {
        }

        public void Dispose()
        {
            _bioContext.Database.EnsureDeleted();
            _bioContext.Dispose();
        }
    }
}