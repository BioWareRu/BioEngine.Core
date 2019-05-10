using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Users;

namespace BioEngine.Core.Entities
{
    [Table("Posts")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class Post : BaseSiteEntity, ISectionEntity, ITaggedContentEntity
    {
        [Required] public virtual int AuthorId { get; set; }
        public bool IsPinned { get; set; } = false;
        public virtual Guid[] SectionIds { get; set; } = new Guid[0];
        public virtual Guid[] TagIds { get; set; } = new Guid[0];

        [InverseProperty(nameof(ContentBlock.Post))]
        public List<ContentBlock> Blocks { get; set; } = new List<ContentBlock>();

        [NotMapped] public string PublicUrl => $"/posts/{Url}.html";

        [NotMapped] public IUser Author { get; set; }
        [NotMapped] public List<Section> Sections { get; set; }
        [NotMapped] public List<Tag> Tags { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
