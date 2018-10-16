using System;

namespace BioEngine.Core.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public SettingType Type { get; set; } = SettingType.String;
        public bool IsRequired { get; set; }
    }
}