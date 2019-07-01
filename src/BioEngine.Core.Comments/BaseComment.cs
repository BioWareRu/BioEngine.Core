using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using JetBrains.Annotations;

namespace BioEngine.Core.Comments
{
    [UsedImplicitly]
    public abstract class BaseComment : BaseEntity, IRoutable
    {
        [Required] public Guid ContentId { get; set; }
        [Required] public string AuthorId { get; set; }
        [Required] public Guid[] SiteIds { get; set; } = new Guid[0];

        [NotMapped] public IUser? Author { get; set; }

        [ForeignKey(nameof(ContentId))] public ContentItem ContentItem { get; set; }
        [NotMapped] public string PublicRouteName { get; set; } = BioEngineCommentsRoutes.Comment;
        [NotMapped] public string Url { get; set; }
    }
}
