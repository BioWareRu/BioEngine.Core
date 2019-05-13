using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BioEngine.Core.Entities;

namespace BioEngine.Core.DB
{
    public class BioEntityMetadataManager
    {
        private readonly List<EntityMetadata> _blocks = new List<EntityMetadata>();
        private readonly List<EntityMetadata> _sections = new List<EntityMetadata>();

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

        public EntityMetadata[] GetBlocksMetadata()
        {
            return _blocks.ToArray();
        }

        public EntityMetadata[] GetSectionsMetadata()
        {
            return _sections.ToArray();
        }
    }


    public struct EntityMetadata
    {
        public EntityMetadata(Type objectType, string entityType, Type dataType)
        {
            ObjectType = objectType;
            Type = entityType;
            DataType = dataType;
        }

        public Type ObjectType { get; }
        public string Type { get; }
        public Type DataType { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TypedEntityAttribute : Attribute
    {
        public TypedEntityAttribute(string type)
        {
            Type = type;
        }

        public string Type { get; set; }
    }
}
