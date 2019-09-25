using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Social
{
    public interface IContentPublisher<TConfig> where TConfig : IContentPublisherConfig
    {
        Task<bool> PublishAsync(ContentItem entity, TConfig config, bool needUpdate, Site? site = null);
        Task<bool> DeleteAsync(ContentItem entity, TConfig config, Site? site = null);
    }
}
