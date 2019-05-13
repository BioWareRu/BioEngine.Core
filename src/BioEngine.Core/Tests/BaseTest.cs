using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BioEngine.Core.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

    public abstract class BaseTestScope : IDisposable
    {
        public void Configure(string dbName, ITestOutputHelper testOutputHelper)
        {
            var bioEngine = new BioEngine(new string[0]);
            bioEngine.ConfigureServices(collection =>
            {
                collection.AddLogging(o => o.AddProvider(new XunitLoggerProvider(testOutputHelper)));
                ConfigureServices(collection, dbName);
            }).AddModule<InMemoryDatabaseModule, InMemoryDatabaseModuleConfig>((configuration, environment) =>
            {
                return new InMemoryDatabaseModuleConfig(dbName);
            });
            bioEngine = ConfigureBioEngine(bioEngine);
            ServiceProvider = bioEngine.Build().Services;
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
