using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BioEngine.Core.DB;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
                if(_scopes[name] is T typedScope)
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

    public abstract class BaseTestScope : IDisposable
    {
        private static readonly BioEntitiesManager EntitiesManager = new BioEntitiesManager();

        public void Configure(string dbName, ITestOutputHelper testOutputHelper)
        {
            Configuration = new ConfigurationBuilder()
                .AddUserSecrets("bw")
                .AddEnvironmentVariables()
                .Build();
            var services = new ServiceCollection();
            services.AddLogging(o => o.AddProvider(new XunitLoggerProvider(testOutputHelper)));
            ConfigureServices(services, dbName);
            ServiceProvider = services.BuildServiceProvider();
        }

        protected void RegisterCoreModule(IServiceCollection services, string scopeName,
            IEnumerable<Assembly> assemblies)
        {
            if(Configuration == null)
            {
                throw new Exception("Configuration is empty");
            }
            bool.TryParse(Configuration["BE_TESTS_POSTGRES"], out var testWithPostgres);
            var module = new CoreModule();
            module.Configure(config =>
            {
                config.Assemblies.AddRange(assemblies);
                config.EnableValidation = true;
                if (testWithPostgres)
                {
                    config.EnableDatabase = true;
                }
                else
                {
                    config.EnableInMemoryDatabase = true;
                    config.InMemoryDatabaseName = scopeName;
                }
            });
            module.ConfigureServices(services, Configuration,
                new HostingEnvironment {EnvironmentName = "Development"});
            module.RegisterEntities(EntitiesManager);
            services.TryAddSingleton(EntitiesManager);
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

        protected IConfiguration? Configuration;
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
