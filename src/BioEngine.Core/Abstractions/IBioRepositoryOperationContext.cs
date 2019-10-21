using System;

namespace BioEngine.Core.Abstractions
{
    public interface IBioRepositoryOperationContext
    {
        Guid? SiteId { get; set; }
    }
}
