using System;
using BioEngine.Core.Users;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryOperationContext : IBioRepositoryOperationContext
    {
        public IUser? User { get; set; }
        public Guid? SiteId { get; set; }
    }
}
