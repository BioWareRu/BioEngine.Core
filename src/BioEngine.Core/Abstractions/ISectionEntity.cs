using System;
using System.Collections.Generic;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Abstractions
{
    public interface ISectionEntity : IEntity
    {
        Guid[] SectionIds { get; set; }
        Guid[] TagIds { get; set; }
        List<Tag> Tags { get; set; }
    }
}
