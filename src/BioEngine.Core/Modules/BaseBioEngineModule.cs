using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Modules
{
    public abstract class BaseBioEngineModule : IBioEngineModule
    {
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
        }

        public virtual void ConfigureHostBuilder(IHostBuilder hostBuilder)
        {
        }

        public virtual AssemblyScanner RegisterValidation()
        {
            return AssemblyScanner.FindValidatorsInAssembly(GetType().Assembly);
        }

        public virtual void ConfigureEntities(IServiceCollection serviceCollection,
            BioEntitiesManager entitiesManager)
        {
            RegisterRepositories(GetType().Assembly, serviceCollection, entitiesManager);
        }

        public virtual Task InitAsync(IServiceProvider serviceProvider, IConfiguration configuration,
            IHostEnvironment environment)
        {
            return Task.CompletedTask;
        }

        protected void RegisterRepositories(Assembly assembly, IServiceCollection serviceCollection,
            BioEntitiesManager entitiesManager)
        {
            var types = new HashSet<TypeInfo>();
            foreach (var definedType in assembly.DefinedTypes)
            {
                types.Add(definedType);
            }

            foreach (var type in types)
            {
                entitiesManager.Register(type);
            }

            serviceCollection.Scan(s =>
                s.FromAssemblies(GetType().Assembly).AddClasses(classes => classes.AssignableTo<IBioRepository>())
                    .AsSelfWithInterfaces());
        }

        protected virtual void CheckConfig()
        {
        }
    }

    public abstract class BaseBioEngineModule<TConfig> : BaseBioEngineModule, IBioEngineModule<TConfig>
        where TConfig : class
    {
        protected TConfig Config { get; private set; }

        public virtual void Configure(Func<IConfiguration, IHostEnvironment, TConfig> configure,
            IConfiguration configuration, IHostEnvironment environment)
        {
            Config = configure(configuration, environment);
            CheckConfig();
        }
    }
}
