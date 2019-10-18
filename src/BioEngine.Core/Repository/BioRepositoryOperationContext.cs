using System;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryOperationContext : IBioRepositoryOperationContext
    {
        public Guid? SiteId { get; set; }
    }
}
