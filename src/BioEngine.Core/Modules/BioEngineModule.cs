﻿using System;
using System.Collections.Generic;
using System.Reflection;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
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

        public virtual void ConfigureDbContext(BioEntitiesManager entitiesManager)
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

        public virtual void RegisterRepositories(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager)
        {
            RegisterRepositories(GetType().Assembly, serviceCollection, entitiesManager);
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

    public abstract class BioEngineModule<TConfig> : BioEngineModule, IBioEngineModule<TConfig> where TConfig : class
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
