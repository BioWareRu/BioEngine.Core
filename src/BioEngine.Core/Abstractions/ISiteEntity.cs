using System;

namespace BioEngine.Core.Abstractions
{
    public interface ISiteEntity : IEntity
    {
        Guid[] SiteIds { get; set; }
    }
}
