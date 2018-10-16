using JetBrains.Annotations;

namespace BioEngine.Core.Settings
{
    [PublicAPI]
    public class SettingsOption
    {
        public SettingsOption(string title, object value, string group)
        {
            Title = title;
            Value = value;
            Group = group;
        }

        public string Title { get; }
        public object Value { get; }
        public string Group { get; }
    }
}