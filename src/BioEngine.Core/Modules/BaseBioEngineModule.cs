using System;
using System.Linq;
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
            var method = entitiesManager.GetType().GetMethod(nameof(BioEntitiesManager.RegisterEntity),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var definedType in assembly.DefinedTypes.Where(type => !type.IsAbstract && typeof(IEntity).IsAssignableFrom(type)))
            {
                method.MakeGenericMethod(definedType).Invoke(entitiesManager, null);
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
