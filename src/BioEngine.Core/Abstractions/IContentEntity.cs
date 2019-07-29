using System;
using System.Collections.Generic;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Abstractions
{
    public interface IContentEntity : ISiteEntity, IRoutable
    {
        string Title { get; set; }
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
