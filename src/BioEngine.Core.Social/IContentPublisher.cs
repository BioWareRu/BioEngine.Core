using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Social
{
    public interface IContentPublisher<TConfig> where TConfig : IContentPublisherConfig
    {
        Task<bool> PublishAsync(IContentItem entity, TConfig config, bool needUpdate, Site site, bool allSites = false);
        Task<bool> DeleteAsync(IContentItem entity, TConfig config, Site site, bool allSites = false);
    }
}
