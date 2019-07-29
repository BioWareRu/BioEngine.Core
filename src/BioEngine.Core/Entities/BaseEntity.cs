using System;
using System.ComponentModel.DataAnnotations;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public abstract class BaseEntity : IBioEntity
    {
        [Key] public virtual Guid Id { get; set; } = Guid.Empty;
        [Required] public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;

        public string GetId()
        {
            return Id.ToString();
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

    public abstract class BaseSiteEntity : BaseEntity, ISiteEntity
    {
        public virtual Guid[] SiteIds { get; set; } = new Guid[0];
    }
}
