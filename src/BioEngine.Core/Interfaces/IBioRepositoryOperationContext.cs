using System;

namespace BioEngine.Core.Interfaces
{
    public interface IBioRepositoryOperationContext
    {
        IUser User { get; set; }
        Guid? SiteId { get; set; }
    }
}