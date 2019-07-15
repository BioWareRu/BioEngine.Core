using System;

namespace BioEngine.Core.DB
{
    public struct EntityMetadata
    {
        public EntityMetadata(Type objectType, string entityKey, Type dataType = null)
        {
            ObjectType = objectType;
            Key = entityKey;
            DataType = dataType;
            IsSection = false;
            IsContentItem = false;
            IsContentBlock = false;
            IsBioEntity = false;
        }

        public Type ObjectType { get; }
        public string Key { get; }
        public Type? DataType { get; }

        public bool IsBioEntity { get; private set; }
        public bool IsSection { get; private set; }
        public bool IsContentItem { get; private set; }
        public bool IsContentBlock { get; private set; }

        public void MarkAsBioEntity()
        {
            IsBioEntity = true;
        }

        public void MarkAsSection()
        {
            IsSection = true;
        }

        public void MarkAsContentItem()
        {
            IsContentItem = true;
        }

        public void MarkAsContentBlock()
        {
            IsContentBlock = true;
        }
    }
}
