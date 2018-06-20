using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Entities
{
    [Table("Sections")]
    public abstract class Section : IEntity<int>, ISiteEntity
    {
        [Key] public virtual int Id { get; set; }
        [Required] public virtual int Type { get; set; }
        public virtual int? ParentId { get; set; }
        public virtual int? ForumId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        [Required] public virtual string Logo { get; set; }
        [Required] public virtual string LogoSmall { get; set; }
        public virtual string Description { get; set; }
        [Required] public virtual string ShortDescription { get; set; }
        public virtual string Keywords { get; set; }
        [Required] public virtual string Hashtag { get; set; }
        public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public virtual DateTimeOffset? DatePublished { get; set; }
        public virtual bool IsPublished { get; set; }
        public virtual int[] SiteIds { get; set; }
    }

    public abstract class Section<T> : Section, ITypedEntity<T> where T : TypedData, new()
    {
        public virtual T Data { get; set; } = new T();
    }

    public abstract class SectionData
    {
    }
}