using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioEngine.Core.Settings
{
    public interface ISettingsOptionsResolver
    {
        bool CanResolve(SettingsBase settings);
        Task<List<SettingsOption>> Resolve(SettingsBase settings, string property);
    }
}