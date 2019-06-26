using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.DB
{
    public class BioEntitiesManager
    {
        private readonly Dictionary<string, EntityMetadata> _registrations =
            new Dictionary<string, EntityMetadata>();

        private readonly List<Action<ModelBuilder>> _configureActions = new List<Action<ModelBuilder>>();

        public void RegisterEntity<TEntity>() where TEntity : IEntity
        {
            RegisterEntity(typeof(TEntity));
        }

        private void RegisterEntity(Type entityType)
        {
            if (entityType.IsAbstract || entityType.BaseType == null)
            {
                return;
            }

            var metaData = GetTypeMetadata(entityType);

            if (_registrations.ContainsKey(metaData.Key))
            {
                throw new Exception($"Entity with key {metaData.Key} already registerd");
            }


            if (typeof(Section).IsAssignableFrom(entityType))
            {
                metaData.MarkAsSection();
            }

            else if (typeof(ContentItem).IsAssignableFrom(entityType))
            {
                metaData.MarkAsContentItem();
            }

            else if (typeof(ContentBlock).IsAssignableFrom(entityType))
            {
                metaData.MarkAsContentBlock();
            }

            _registrations.Add(metaData.Key, metaData);
        }

        private static EntityMetadata GetTypeMetadata(Type type)
        {
            Type? dataType = null;
            if (typeof(ITypedEntity).IsAssignableFrom(type))
            {
                dataType = type.BaseType?.GenericTypeArguments[0];

                if (dataType == null)
                {
                    throw new ArgumentException($"Entity type without data type: {type}");
                }
            }

            var typedAttribute = type.GetCustomAttribute<EntityAttribute>();
            if (typedAttribute == null)
            {
                throw new ArgumentException($"Entity type without type attribute: {type}");
            }

            return new EntityMetadata(type, typedAttribute.Key, dataType);
        }

        public IEnumerable<EntityMetadata> GetTypes()
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
            return _registrations.Values.Where(m => m.IsContentBlock).ToArray();
        }

        public EntityMetadata[] GetSectionsMetadata()
        {
            return _registrations.Values.Where(m => m.IsSection).ToArray();
        }

        public EntityMetadata[] GetContentItemsMetadata()
        {
            return _registrations.Values.Where(m => m.IsContentItem).ToArray();
        }

        public string GetKey<T>() where T : IEntity
        {
            return GetKey(typeof(T));
        }

        private string GetKey(Type entityType)
        {
            var key = _registrations.Values.Where(m => m.ObjectType == entityType).Select(m => m.Key)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception($"Can't find key for entity {entityType}");
            }

            return key;
        }

        public string GetKey(IEntity entity)
        {
            return GetKey(entity.GetType());
        }
    }
}
