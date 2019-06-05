using System;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Social
{
    public abstract class BasePublishRecord : BaseSiteEntity
    {
        [NotMapped] public override string Title { get; set; }
        [NotMapped] public override string Url { get; set; }
        public Guid ContentId { get; set; }
        public string Type { get; set; }
    }
}