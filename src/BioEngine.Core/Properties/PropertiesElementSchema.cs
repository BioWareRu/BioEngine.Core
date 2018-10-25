namespace BioEngine.Core.Properties
{
    public class PropertiesElementSchema
    {
        public PropertiesElementSchema(string key, string name, PropertyElementType type, bool isRequired)
        {
            Key = key;
            Name = name;
            Type = type;
            IsRequired = isRequired;
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public PropertyElementType Type { get; set; }
        public bool IsRequired { get; set; }
    }
}