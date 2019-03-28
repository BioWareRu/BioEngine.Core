using System;
using BioEngine.Core.Users;

namespace BioEngine.Core.Repository
{
    public interface IBioRepositoryOperationContext
    {
        IUser User { get; set; }
        Guid? SiteId { get; set; }
    }
}