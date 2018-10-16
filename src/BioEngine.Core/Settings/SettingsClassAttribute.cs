using System;

namespace BioEngine.Core.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsClassAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public SettingsMode Mode { get; set; } = SettingsMode.OnePerEntity;
    }
}