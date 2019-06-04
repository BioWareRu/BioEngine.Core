using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.DB
{
    public class BioEntitiesManager
    {
        private readonly Dictionary<string, BioEntityRegistration> _registrations =
            new Dictionary<string, BioEntityRegistration>();

        private readonly List<Action<ModelBuilder>> _configureActions = new List<Action<ModelBuilder>>();
        private readonly List<EntityMetadata> _blocks = new List<EntityMetadata>();
        private readonly List<EntityMetadata> _sections = new List<EntityMetadata>();
        private readonly List<EntityMetadata> _contentItems = new List<EntityMetadata>();

        public void Register<TEntity>(Action<ModelBuilder>? configureContext = null) where TEntity : IEntity
        {
            if (!_registrations.ContainsKey(typeof(TEntity).FullName))
            {
                _registrations.Add(typeof(TEntity).FullName,
                    new BioEntityRegistration(typeof(TEntity), configureContext));
            }
        }
        
        public void Register(Type type)
        {
            if (type.IsAbstract || type.BaseType == null)
            {
                return;
            }

            if (typeof(Section).IsAssignableFrom(type))
            {
                var metaData = GetTypeMetadata(type);
                if (_sections.Any(m => m.Type == metaData.Type))
                {
                    throw new Exception($"Section with type {metaData.Type} already registered");
                }

                _sections.Add(metaData);
            }

            if (typeof(ContentItem).IsAssignableFrom(type))
            {
                var metaData = GetTypeMetadata(type);
                if (_contentItems.Any(m => m.Type == metaData.Type))
                {
                    throw new Exception($"Content item with type {metaData.Type} already registered");
                }

                _contentItems.Add(metaData);
            }

            if (typeof(ContentBlock).IsAssignableFrom(type))
            {
                var metaData = GetTypeMetadata(type);
                if (_blocks.Any(m => m.Type == metaData.Type))
                {
                    throw new Exception($"Block with type {metaData.Type} already registered");
                }

                _blocks.Add(metaData);
            }
        }
        
        private static EntityMetadata GetTypeMetadata(Type type)
        {
            var dataType = type.BaseType?.GenericTypeArguments[0];

            if (dataType == null)
            {
                throw new ArgumentException($"Entity type without data type: {type}");
            }

            var typedAttribute = type.GetCustomAttribute<TypedEntityAttribute>();
            if (typedAttribute == null)
            {
                throw new ArgumentException($"Entity type without type attribute: {type}");
            }

            return new EntityMetadata(type, typedAttribute.Type, dataType);
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
        
        public EntityMetadata[] GetBlocksMetadata()
        {
            return _blocks.ToArray();
        }

        public EntityMetadata[] GetSectionsMetadata()
        {
            return _sections.ToArray();
        }

        public EntityMetadata[] GetContentItemsMetadata()
        {
            return _contentItems.ToArray();
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
