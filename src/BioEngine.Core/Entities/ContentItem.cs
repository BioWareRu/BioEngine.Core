using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Users;

namespace BioEngine.Core.Entities
{
    [Table("Content")]
    public abstract class ContentItem : BaseEntity, ITaggedContentEntity, ISectionEntity
    {
        [Required] public virtual string Type { get; set; }
        public virtual bool IsPublished { get; set; }
        public virtual DateTimeOffset? DatePublished { get; set; }
        public Guid[] SiteIds { get; set; } = new Guid[0];
        public List<ContentBlock> Blocks { get; set; } = new List<ContentBlock>();
        public Guid[] SectionIds { get; set; } = new Guid[0];
        public Guid[] TagIds { get; set; } = new Guid[0];

        [Required] public virtual int AuthorId { get; set; }
        [NotMapped] public IUser Author { get; set; }
        [NotMapped] public List<Section> Sections { get; set; }
        [NotMapped] public List<Tag> Tags { get; set; }
        [NotMapped] public abstract string PublicRouteName { get; set; }
    }

    public abstract class ContentItem<T> : ContentItem, ITypedEntity<T> where T : ITypedData, new()
    {
        [NotMapped] public abstract string TypeTitle { get; }
        public T Data { get; set; }
    }
}
