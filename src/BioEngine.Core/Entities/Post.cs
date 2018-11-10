using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Entities
{
    [Table("Posts")]
    public class Post : BaseSiteEntity<int>, ISectionEntity<int>, IContentEntity<int>,
        IRoutable
    {
        [Required] public virtual int AuthorId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        public bool IsPinned { get; set; } = false;
        public virtual int[] SectionIds { get; set; } = new int[0];
        public virtual int[] TagIds { get; set; } = new int[0];

        [InverseProperty(nameof(PostBlock.Post))]
        public List<PostBlock> Blocks { get; set; }
        
        [NotMapped] public string PublicUrl => $"/content/{Id}-{Url}.html";
        [NotMapped] public IUser Author { get; set; }
    }
}