using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Entities
{
    public abstract class BaseEntity : IEntity
    {
        [Key] public virtual Guid Id { get; set; } = Guid.Empty;
        [Required] public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public virtual bool IsPublished { get; set; }
        public virtual DateTimeOffset? DatePublished { get; set; }
        [NotMapped] public virtual List<PropertiesEntry> Properties { get; set; } = new List<PropertiesEntry>();
    }

    public abstract class BaseSiteEntity : BaseEntity, ISiteEntity
    {
        public virtual Guid[] SiteIds { get; set; } = new Guid[0];
    }

    public abstract class BaseSingleSiteEntity : BaseEntity, ISingleSiteEntity
    {
        public virtual Guid SiteId { get; set; }
    }
}
