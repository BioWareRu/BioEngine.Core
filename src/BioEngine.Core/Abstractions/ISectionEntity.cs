using System;

namespace BioEngine.Core.Abstractions
{
    public interface ISectionEntity : IEntity
    {
        Guid[] SectionIds { get; set; }
        Guid[] TagIds { get; set; }
    }
}