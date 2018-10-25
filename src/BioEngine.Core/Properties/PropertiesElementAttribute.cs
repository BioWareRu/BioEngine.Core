using System;

namespace BioEngine.Core.Properties
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertiesElementAttribute : Attribute
    {
        public string Name { get; set; }
        public PropertyElementType Type { get; set; } = PropertyElementType.String;
        public bool IsRequired { get; set; }
    }
}