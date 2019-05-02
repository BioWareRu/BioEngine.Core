using System;
using System.Collections.Generic;

namespace BioEngine.Core.Entities
{
    public interface ITaggedContentEntity : IContentEntity
    {
        Guid[] TagIds { get; set; }
        List<Tag> Tags { get; set; }
    }
}