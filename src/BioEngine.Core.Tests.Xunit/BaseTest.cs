using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BioEngine.Core.DB;
using BioEngine.Core.Db.InMemory;
using BioEngine.Core.Db.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests.Xunit
{
    public abstract class BaseTest
    {
        protected ITestOutputHelper TestOutputHelper { get; }

        protected BaseTest(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }
    }

    public abstract class BaseTest<T> : BaseTest, IDisposable where T : BaseTestScope<T>
    {
        protected BaseTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        private readonly Dictionary<string, BaseTestScope<T>> _scopes = new Dictionary<string, BaseTestScope<T>>();

        protected T GetScope([CallerMemberName] string name = "")
        {
            T scope;

            if (!_scopes.ContainsKey(name))
            {
                scope = Activator.CreateInstance<T>();
                scope.Configure(name, TestOutputHelper);
                scope.OnCreated();
                _scopes.Add(name, scope);
            }
            else
            {
                if (_scopes[name] is T typedScope)
                {
                    scope = typedScope;
                }
                else
                {
                    throw new Exception($"Can't create scope for {name}");
                }
            }


            return scope;
        }


        protected BioContext GetDbContext(string name, bool init = true)
        {
            return GetScope(name).GetDbContext();
        }

        public void Dispose()
        {
            foreach (var testScope in _scopes)
            {
                testScope.Value.Dispose();
            }
        }
    }

    public abstract class BaseTestScope<TScope> : IDisposable where TScope : class
    {
        public void Configure(string dbName, ITestOutputHelper testOutputHelper)
        {
            var bioEngine = new BioEngine(new string[0]);

            bioEngine.ConfigureServices(collection =>
                {
                    collection.AddLogging(o => o.AddProvider(new XunitLoggerProvider(testOutputHelper)));
                    ConfigureServices(collection, dbName);
                })
                .AddEntities();

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<TScope>()
                .Build();

            var testInMemory =
                !string.IsNullOrEmpty(config["BIOENGINE_TEST_IN_MEMORY"])
                && bool.TryParse(config["BIOENGINE_TEST_IN_MEMORY"], out var outBool) && outBool;
            if (testInMemory)
            {
                bioEngine.AddModule<InMemoryDatabaseModule<BioContext>, InMemoryDatabaseModuleConfig>(
                    (configuration, environment) => new InMemoryDatabaseModuleConfig(dbName));
            }
            else
            {
                bioEngine.AddModule<PostgresDatabaseModule<BioContext>, PostgresDatabaseModuleConfig>((configuration,
                    environment) =>
                {
                    return new PostgresDatabaseModuleConfig(config["BE_POSTGRES_HOST"],
                        config["BE_POSTGRES_USERNAME"], dbName,
                        config["BE_POSTGRES_PASSWORD"],
                        int.Parse(config["BE_POSTGRES_PORT"])) {EnableSensitiveLogging = true};
                });
            }

            bioEngine = ConfigureBioEngine(bioEngine);
            ServiceProvider = bioEngine.GetServices();
        }

        protected virtual BioEngine ConfigureBioEngine(BioEngine bioEngine)
        {
            return bioEngine;
        }

        protected virtual IServiceCollection ConfigureServices(IServiceCollection services, string name)
        {
            return services;
        }

        private BioContext? _bioContext;

        public virtual void OnCreated()
        {
            _bioContext = ServiceProvider.GetService<BioContext>();
            if (_bioContext == null)
            {
                throw new Exception("Can't create db context");
            }

            _bioContext.Database.EnsureDeleted();
            _bioContext.Database.EnsureCreated();
            InitDbContext(_bioContext);
        }


        public BioContext GetDbContext()
        {
            if (_bioContext == null)
            {
                throw new Exception("Db context is null");
            }

            return _bioContext;
        }

        protected IServiceProvider? ServiceProvider;

        protected virtual void InitDbContext(BioContext dbContext)
        {
        }

        protected virtual void ModifyDbOptions(DbContextOptions options, bool inMemory)
        {
        }

        public T Get<T>()
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public IServiceScope CreateScope()
        {
            return ServiceProvider.CreateScope();
        }

        public ILogger<T> GetLogger<T>()
        {
            return ServiceProvider.GetRequiredService<ILogger<T>>();
        }


        public void Dispose()
        {
            GetDbContext().Database.EnsureDeleted();
            GetDbContext().Dispose();
        }
    }
}
