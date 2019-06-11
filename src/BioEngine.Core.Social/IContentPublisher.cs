using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Social
{
    public interface IContentPublisher<TConfig> where TConfig : IContentPublisherConfig
    {
        Task<bool> PublishAsync(ContentItem entity, Site site, TConfig config);
        Task<bool> DeleteAsync(ContentItem entity, TConfig config, Site? site = null);
    }
}
