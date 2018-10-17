using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioEngine.Core.Settings
{
    public interface ISettingsOptionsResolver
    {
        bool CanResolve(SettingsBase settings);
        Task<List<SettingsOption>> ResolveAsync(SettingsBase settings, string property);
    }
}