using System;
using System.Collections.Generic;
using System.Reflection;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Modules
{
    public abstract class BioEngineModule : IBioEngineModule
    {
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
        }

        public virtual void ConfigureHostBuilder(IHostBuilder hostBuilder)
        {
        }

        public virtual void RegisterEntities(BioEntitiesManager entitiesManager)
        {
        }

        public virtual void RegisterValidation(IServiceCollection serviceCollection)
        {
            var validators = AssemblyScanner.FindValidatorsInAssembly(GetType().Assembly);
            foreach (var validator in validators)
            {
                serviceCollection.AddScoped(validator.InterfaceType, validator.ValidatorType);
            }
        }

        public void RegisterRepositories(IServiceCollection serviceCollection, BioEntityMetadataManager metadataManager)
        {
            var types = new HashSet<TypeInfo>();
            foreach (var definedType in GetType().Assembly.DefinedTypes)
            {
                types.Add(definedType);
            }

            foreach (var type in types)
            {
                metadataManager.Register(type);
            }

            serviceCollection.Scan(s =>
                s.FromAssemblies(GetType().Assembly).AddClasses(classes => classes.AssignableTo<IBioRepository>())
                    .AsSelfWithInterfaces());
        }

        protected virtual void CheckConfig()
        {
        }
    }

    public abstract class BioEngineModule<TConfig> : BioEngineModule, IBioEngineModule<TConfig> where TConfig : new()
    {
        protected readonly TConfig Config = new TConfig();

        public virtual void Configure(Action<TConfig, IConfiguration, IHostEnvironment> config,
            IConfiguration configuration, IHostEnvironment environment)
        {
            config?.Invoke(Config, configuration, environment);
            CheckConfig();
        }
    }
}
