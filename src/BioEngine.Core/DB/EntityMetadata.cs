using System;

namespace BioEngine.Core.DB
{
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
}
