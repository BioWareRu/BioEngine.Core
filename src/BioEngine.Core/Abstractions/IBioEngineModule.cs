﻿using System;
using BioEngine.Core.DB;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Abstractions
{
    public interface IBioEngineModule
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment);

        AssemblyScanner RegisterValidation();
        void ConfigureEntities(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager);
    }

    public interface IBioEngineModule<TConfig> : IBioEngineModule where TConfig : class
    {
        void Configure(Func<IConfiguration, IHostEnvironment, TConfig> configure, IConfiguration configuration,
            IHostEnvironment environment);
    }
}