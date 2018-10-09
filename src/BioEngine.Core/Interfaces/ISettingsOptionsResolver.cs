using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Providers;

namespace BioEngine.Core.Interfaces
{
    public interface ISettingsOptionsResolver
    {
        bool CanResolve(SettingsBase settings);
        Task<List<SettingsOption>> Resolve(SettingsBase settings, string property);
    }

    public class SettingsOption
    {
        public SettingsOption(string title, object value, string @group)
        {
            Title = title;
            Value = value;
            Group = @group;
        }

        public string Title { get; }
        public object Value { get; }
        public string Group { get; }
    }
}