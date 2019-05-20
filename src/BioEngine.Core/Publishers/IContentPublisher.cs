using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Publishers
{
    public interface IContentPublisher<TConfig> where TConfig : IContentPublisherConfig
    {
        Task<bool> PublishAsync(IContentEntity entity, Site site, TConfig config);
        Task<bool> DeleteAsync(IContentEntity entity, TConfig config, Site site = null);
    }

    public interface IContentPublisherConfig
    {
    }

    public abstract class BasePublishRecord : BaseSiteEntity
    {
        [NotMapped] public override string Title { get; set; }
        [NotMapped] public override string Url { get; set; }
        public Guid ContentId { get; set; }
        public string Type { get; set; }
    }
}
