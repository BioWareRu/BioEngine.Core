using System;

namespace BioEngine.Core.Entities
{
    public interface ISectionEntity : IEntity
    {
        Guid[] SectionIds { get; set; }
        Guid[] TagIds { get; set; }
    }
}