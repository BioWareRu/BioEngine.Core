namespace BioEngine.Core.Settings
{
    public class SettingsPropertySchema
    {
        public SettingsPropertySchema(string key, string name, SettingType type, bool isRequired)
        {
            Key = key;
            Name = name;
            Type = type;
            IsRequired = isRequired;
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public SettingType Type { get; set; }
        public bool IsRequired { get; set; }
    }
}