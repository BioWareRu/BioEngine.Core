using System;

namespace BioEngine.Core.Properties
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertiesSetAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public PropertiesQuantity Quantity { get; set; } = PropertiesQuantity.OnePerEntity;

        public PropertiesSetAttribute(string name)
        {
            Name = name;
        }
    }
}
