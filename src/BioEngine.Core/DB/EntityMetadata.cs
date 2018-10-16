using System;

namespace BioEngine.Core.DB
{
    public struct EntityMetadata
    {
        public EntityMetadata(Type entityType, Type dataType)
        {
            EntityType = entityType;
            DataType = dataType;
        }

        public Type EntityType { get; }
        public Type DataType { get; }
    }
}