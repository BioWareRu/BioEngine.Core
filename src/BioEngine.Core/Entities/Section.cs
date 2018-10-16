using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Settings;
using BioEngine.Core.Storage;

namespace BioEngine.Core.Entities
{
    [Table("Sections")]
    public abstract class Section : IEntity<int>, ISiteEntity<int>, IRoutable
    {
        public object GetId() => Id;

        [Key] public virtual int Id { get; set; }
        [Required] public virtual string Type { get; set; }
        public virtual int? ParentId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        [Required] public virtual StorageItem Logo { get; set; }
        [Required] public virtual StorageItem LogoSmall { get; set; }
        [Required] public virtual string ShortDescription { get; set; }
        [Required] public virtual string Hashtag { get; set; }
        public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public virtual DateTimeOffset? DatePublished { get; set; }
        public virtual bool IsPublished { get; set; }
        public virtual int[] SiteIds { get; set; } = new int[0];

        [NotMapped]
        public List<SettingsEntry> Settings { get; set; } = new List<SettingsEntry>();

        [NotMapped] public string PublicUrl => $"/section/{Id}-{Url}.html";
    }

    public abstract class Section<T> : Section, ITypedEntity<T> where T : TypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped] public abstract string TypeTitle { get; set; }
    }
}