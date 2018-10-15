using System;

namespace BioEngine.Core.DB
{
    public struct EntityMetadata
    {
        public EntityMetadata(Type type, int discriminator, Type dataType, EntityMetadataType entityType)
        {
            Type = type;
            Discriminator = discriminator;
            DataType = dataType;
            EntityType = entityType;
        }

        public Type Type { get; }
        public int Discriminator { get; }
        public Type DataType { get; }
        public EntityMetadataType EntityType { get; }
    }
}