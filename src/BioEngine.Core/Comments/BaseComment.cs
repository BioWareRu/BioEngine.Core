using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using JetBrains.Annotations;

namespace BioEngine.Core.Comments
{
    [UsedImplicitly]
    public abstract class BaseComment : BaseEntity
    {
        [Required] public Guid ContentId { get; set; }
        [Required] public int AuthorId { get; set; }
        [Required] public Guid[] SiteIds { get; set; } = new Guid[0];

        [NotMapped] public abstract string PublicUrl { get; set; }
        [NotMapped] public IUser? Author { get; set; }

        [ForeignKey(nameof(ContentId))] public ContentItem ContentItem { get; set; }
    }
}
