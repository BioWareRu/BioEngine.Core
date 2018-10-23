using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Settings;

namespace BioEngine.Core.Entities
{
    public abstract class BaseEntity<T> : IEntity<T>
    {
        [Key] public virtual T Id { get; set; }
        [Required] public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public virtual bool IsPublished { get; set; }
        public virtual DateTimeOffset? DatePublished { get; set; }

        public virtual object GetId() => Id;

        [NotMapped] public virtual List<SettingsEntry> Settings { get; set; } = new List<SettingsEntry>();
    }

    public abstract class BaseSiteEntity<T> : BaseEntity<T>, ISiteEntity<T>
    {
        public virtual int[] SiteIds { get; set; } = new int[0];
    }
}