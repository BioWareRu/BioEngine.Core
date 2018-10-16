using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;

namespace BioEngine.Core.Entities
{
    [Table("Content")]
    public abstract class ContentItem : IEntity<int>, ISiteEntity<int>, ISectionEntity<int>, IContentEntity<int>,
        IRoutable
    {
        public object GetId() => Id;

        [Key] public virtual int Id { get; set; }
        public virtual string Type { get; set; }
        [Required] public virtual int AuthorId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        [Required] public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public virtual DateTimeOffset? DatePublished { get; set; }
        public virtual bool IsPublished { get; set; } = false;
        public bool IsPinned { get; set; } = false;
        public virtual int[] SectionIds { get; set; } = new int[0];
        public virtual int[] SiteIds { get; set; } = new int[0];
        public virtual int[] TagIds { get; set; } = new int[0];

        [NotMapped] public List<SettingsEntry> Settings { get; set; } = new List<SettingsEntry>();

        [NotMapped] public string PublicUrl => $"/content/{Id}-{Url}.html";
    }

    public abstract class ContentItem<T> : ContentItem, ITypedEntity<T> where T : TypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped] public abstract string TypeTitle { get; set; }
    }
}