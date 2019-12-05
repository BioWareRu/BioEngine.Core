using System;
using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Properties;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryContext<T> : IDisposable where T : class, IEntity
    {
        private readonly IServiceScope _scope;
        internal BioContext DbContext { get; }
        public List<IValidator<T>>? Validators { get; }
        public PropertiesProvider PropertiesProvider { get; }
        public BioRepositoryHooksManager HooksManager { get; }

        public BioRepositoryContext(IServiceScopeFactory serviceScopeFactory)
        {
            _scope = serviceScopeFactory.CreateScope();
            DbContext = _scope.ServiceProvider.GetRequiredService<BioContext>();
            PropertiesProvider = _scope.ServiceProvider.GetRequiredService<PropertiesProvider>();
            HooksManager = _scope.ServiceProvider.GetRequiredService<BioRepositoryHooksManager>();
            Validators = _scope.ServiceProvider.GetServices<IValidator<T>>()?.ToList() ?? new List<IValidator<T>>();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
