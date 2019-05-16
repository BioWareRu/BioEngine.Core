using System;

namespace BioEngine.Core.Entities
{
    public interface ISiteEntity : IEntity
    {
        Guid[] SiteIds { get; set; }
        Guid MainSiteId { get; set; }
    }
}
