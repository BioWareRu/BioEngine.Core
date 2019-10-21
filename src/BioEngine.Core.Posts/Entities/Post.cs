using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Routing;
using BioEngine.Core.Users;

namespace BioEngine.Core.Posts.Entities
{
    [Entity("postcontentitem")]
    [Table("Posts")]
    public class Post<TUserPk> : BaseEntity, IContentItem
    {
        public TUserPk AuthorId { get; set; }
        [NotMapped] public IUser<TUserPk> Author { get; set; }
        [NotMapped] public string PublicRouteName { get; set; } = BioEnginePostsRoutes.Post;
        public string Url { get; set; }
        public Guid[] SiteIds { get; set; } = new Guid[0];
        public string Title { get; set; }
        public List<ContentBlock> Blocks { get; set; } = new List<ContentBlock>();
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        public Guid[] SectionIds { get; set; } = new Guid[0];
        public Guid[] TagIds { get; set; } = new Guid[0];
        public List<Section> Sections { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
