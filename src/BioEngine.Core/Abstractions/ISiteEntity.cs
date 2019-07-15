using System;

namespace BioEngine.Core.Abstractions
{
    public interface ISiteEntity : IBioEntity
    {
        Guid[] SiteIds { get; set; }
    }
}
