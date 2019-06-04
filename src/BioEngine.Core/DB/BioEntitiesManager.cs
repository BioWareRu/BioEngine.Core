using System;
using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.DB
{
    public class BioEntitiesManager
    {
        private readonly Dictionary<string, BioEntityRegistration> _registrations =
            new Dictionary<string, BioEntityRegistration>();

        private readonly List<Action<ModelBuilder>> _configureActions = new List<Action<ModelBuilder>>();

        public void Register<TEntity>(Action<ModelBuilder>? configureContext = null) where TEntity : IEntity
        {
            if (!_registrations.ContainsKey(typeof(TEntity).FullName))
            {
                _registrations.Add(typeof(TEntity).FullName,
                    new BioEntityRegistration(typeof(TEntity), configureContext));
            }
        }

        public IEnumerable<BioEntityRegistration> GetTypes()
        {
            return _registrations.Values;
        }

        public void ConfigureDbContext(Action<ModelBuilder> configureContext)
        {
            _configureActions.Add(configureContext);
        }

        public IEnumerable<Action<ModelBuilder>> GetConfigureActions()
        {
            return _configureActions.ToList();
        }
    }

    public class BioEntityRegistration
    {
        public Type Type { get; }
        public Action<ModelBuilder>? ConfigureContext { get; }

        public BioEntityRegistration(Type type, Action<ModelBuilder>? configureContext = null)
        {
            ConfigureContext = configureContext;
            Type = type;
        }
    }
}
