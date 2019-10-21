using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using JetBrains.Annotations;

namespace BioEngine.Core.Comments
{
    [UsedImplicitly]
    public abstract class BaseComment<TUserPk> : BaseEntity, IRoutable
    {
        [Required] public string ContentType { get; set; }
        [Required] public Guid ContentId { get; set; }
        [Required] public TUserPk AuthorId { get; set; }
        [Required] public Guid[] SiteIds { get; set; } = new Guid[0];
        public Guid? ReplyTo { get; set; }
        public string? Text { get; set; }

        [NotMapped] public IUser<TUserPk>? Author { get; set; }

        [NotMapped] public IContentItem ContentItem { get; set; }
        [NotMapped] public string PublicRouteName { get; set; } = BioEngineCommentsRoutes.Comment;
        [NotMapped] public string Url { get; set; }
    }
}
