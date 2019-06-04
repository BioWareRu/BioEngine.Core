using System;

namespace BioEngine.Core.DB
{
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