using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using JetBrains.Annotations;

namespace BioEngine.Core.Comments
{
    [UsedImplicitly]
    public abstract class BaseComment : BaseEntity, ISiteEntity
    {
        [Required] public Guid ContentId { get; set; }
        [Required] public string Type { get; set; }

        [Required] public int AuthorId { get; set; }
        [Required] public Guid[] SiteIds { get; set; }

        [NotMapped] public string PublicUrl { get; set; }
        [NotMapped] public IUser Author { get; set; }
        [NotMapped] public IContentEntity Content { get; set; }
    }
}
