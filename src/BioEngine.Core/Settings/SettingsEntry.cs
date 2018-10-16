using System.Collections.Generic;

namespace BioEngine.Core.Settings
{
    public class SettingsEntry
    {
        public SettingsEntry(string key, SettingsSchema schema)
        {
            Key = key;
            Schema = schema;
        }

        public string Key { get; }

        public SettingsSchema Schema { get; }

        public List<SettingsValue> Settings { get; } = new List<SettingsValue>();
    }
}