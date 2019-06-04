using System;
using System.Collections.Generic;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Abstractions
{
    public interface ITaggedContentEntity : IContentEntity
    {
        Guid[] TagIds { get; set; }
        List<Tag> Tags { get; set; }
    }
}