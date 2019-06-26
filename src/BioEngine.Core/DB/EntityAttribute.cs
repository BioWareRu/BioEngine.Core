using System;

namespace BioEngine.Core.DB
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        public EntityAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
    }
}
