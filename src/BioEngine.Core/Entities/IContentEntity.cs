using System;
using System.Collections.Generic;
using BioEngine.Core.Routing;

namespace BioEngine.Core.Entities
{
    public interface IContentEntity : ISiteEntity, IRoutable
    {
        List<ContentBlock> Blocks { get; set; }
        bool IsPublished { get; set; }
        DateTimeOffset? DatePublished { get; set; }
    }

    public enum ContentEntityViewMode
    {
        List,
        Entity
    }
}
