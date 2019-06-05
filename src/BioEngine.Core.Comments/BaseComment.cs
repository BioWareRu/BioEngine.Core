using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Routing;
using JetBrains.Annotations;

namespace BioEngine.Core.Comments
{
    [UsedImplicitly]
    public abstract class BaseComment : BaseEntity, IRoutable
    {
        [Required] public Guid ContentId { get; set; }
        [Required] public int AuthorId { get; set; }
        [Required] public Guid[] SiteIds { get; set; } = new Guid[0];

        [NotMapped] public IUser? Author { get; set; }

        [ForeignKey(nameof(ContentId))] public ContentItem ContentItem { get; set; }
        [NotMapped] public string PublicRouteName { get; set; } = BioEngineCoreRoutes.Comment;
    }
}
