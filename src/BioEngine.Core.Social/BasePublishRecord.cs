using System;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Social
{
    public abstract class BasePublishRecord : BaseSiteEntity
    {
        public Guid ContentId { get; set; }
        public string Type { get; set; }
    }
}
