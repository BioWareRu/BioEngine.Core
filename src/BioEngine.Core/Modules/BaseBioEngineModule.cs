using System;
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

        public virtual void ConfigureDbContext(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            RegisterDbConfigurators(services, GetType().Assembly);
            RegisterRepositories(services, GetType().Assembly);
        }

        public virtual void RegisterEntities<T>(IServiceCollection services)
        {
            RegisterDbConfigurators(services, typeof(T).Assembly);
            RegisterRepositories(services, typeof(T).Assembly);
        }

        protected virtual void RegisterDbConfigurators(IServiceCollection services, Assembly assembly)
        {
            services.Scan(s =>
                s.FromAssemblies(assembly).AddClasses(classes => classes.AssignableTo<IBioContextModelConfigurator>())
                    .As<IBioContextModelConfigurator>());
        }

        protected virtual void RegisterRepositories<T>(IServiceCollection services)
        {
            RegisterRepositories(services, typeof(T).Assembly);
        }

        protected virtual void RegisterRepositories(IServiceCollection services, Assembly assembly)
        {
            services.Scan(s =>
                s.FromAssemblies(assembly).AddClasses(classes => classes.AssignableTo<IBioRepository>())
                    .AsSelfWithInterfaces());
        }


        public virtual Task InitAsync(IServiceProvider serviceProvider, IConfiguration configuration,
            IHostEnvironment environment)
        {
            return Task.CompletedTask;
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
