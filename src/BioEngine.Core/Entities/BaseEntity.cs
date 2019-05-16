using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public abstract class BaseEntity : IEntity
    {
        [Key] public virtual Guid Id { get; set; } = Guid.Empty;
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        [Required] public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public virtual bool IsPublished { get; set; }
        public virtual DateTimeOffset? DatePublished { get; set; }
        [NotMapped] public virtual List<PropertiesEntry> Properties { get; set; } = new List<PropertiesEntry>();
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

    public abstract class BaseSiteEntity : BaseEntity, ISiteEntity
    {
        public virtual Guid[] SiteIds { get; set; } = new Guid[0];
        [Required] public virtual Guid MainSiteId { get; set; } = Guid.Empty;
    }

    public abstract class BaseSingleSiteEntity : BaseEntity, ISingleSiteEntity
    {
        public virtual Guid SiteId { get; set; }
    }
}
