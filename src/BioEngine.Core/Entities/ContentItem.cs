using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Entities
{
    [Table("Content")]
    public abstract class ContentItem : IEntity<int>, ISiteEntity, ISectionEntity
    {
        [Key] public virtual int Id { get; set; }
        public virtual int Type { get; set; }
        [Required] public virtual int AuthorId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        public virtual string Description { get; set; }
        [Required] public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public virtual DateTimeOffset? DatePublished { get; set; }
        public virtual bool IsPublished { get; set; } = false;
        public bool IsPinned { get; set; } = false;
        public int? ForumTopicId { get; set; }
        public int? ForumPostId { get; set; }
        public int CommentsCount { get; set; }
        public virtual int[] SectionIds { get; set; } = new int[0];
        public virtual int[] SiteIds { get; set; } = new int[0];
    }

    public abstract class ContentItem<T> : ContentItem, ITypedEntity<T> where T : TypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped]
        public virtual string TypeTitle { get; set; }
    }

    public abstract class ContentData
    {
    }

    public class TypedEntityAttribute : Attribute
    {
        public int Type { get; }

        public TypedEntityAttribute(int type)
        {
            Type = type;
        }
    }
}