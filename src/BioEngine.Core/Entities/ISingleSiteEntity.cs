using System;

namespace BioEngine.Core.Entities
{
    public interface ISingleSiteEntity : IEntity
    {
        Guid SiteId { get; set; }
    }
}