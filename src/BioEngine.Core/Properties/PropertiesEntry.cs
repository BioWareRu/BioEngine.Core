using System.Collections.Generic;

namespace BioEngine.Core.Properties
{
    public class PropertiesEntry
    {
        public PropertiesEntry(string key, PropertiesSchema schema)
        {
            Key = key;
            Schema = schema;
        }

        public string Key { get; }

        public PropertiesSchema Schema { get; }

        public List<PropertiesValue> Properties { get; } = new List<PropertiesValue>();
    }
}