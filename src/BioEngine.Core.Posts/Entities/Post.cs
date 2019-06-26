using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Routing;

namespace BioEngine.Core.Posts.Entities
{
    [Entity("postcontentitem")]
    public class Post : ContentItem<PostData>
    {
        public override string TypeTitle { get; } = "Пост";
        [NotMapped] public override string PublicRouteName { get; set; } = BioEnginePostsRoutes.Post;
    }

    public class PostData : ITypedData
    {
    }
}
