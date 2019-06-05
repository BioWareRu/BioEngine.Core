using System;

namespace BioEngine.Core.Abstractions
{
    public interface IBioRepositoryOperationContext
    {
        IUser? User { get; set; }
        Guid? SiteId { get; set; }
    }
}
