using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Users;

namespace BioEngine.Core.Entities
{
    [Table("Posts")]
    public class Post : BaseSiteEntity, ISectionEntity, IContentEntity,
        IRoutable
    {
        [Required] public virtual int AuthorId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        public bool IsPinned { get; set; } = false;
        public virtual Guid[] SectionIds { get; set; } = new Guid[0];
        public virtual Guid[] TagIds { get; set; } = new Guid[0];

        [InverseProperty(nameof(ContentBlock.Post))]
        public List<ContentBlock> Blocks { get; set; }

        [NotMapped] public string PublicUrl => $"/post/{Url}.html";
        [NotMapped] public IUser Author { get; set; }
        [NotMapped] public List<Section> Sections { get; set; }
        [NotMapped] public List<Tag> Tags { get; set; }
    }
}
