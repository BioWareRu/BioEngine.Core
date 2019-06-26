using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Api.Entities;
using BioEngine.Core.Api.Models;
using BioEngine.Core.Repository;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Posts.Api.Entities
{
    public class PostRequestItem : SectionEntityRestModel<Core.Posts.Entities.Post>,
        IContentRequestRestModel<Core.Posts.Entities.Post>
    {
        public List<ContentBlock> Blocks { get; set; }

        public async Task<Core.Posts.Entities.Post> GetEntityAsync(Core.Posts.Entities.Post entity)
        {
            return await FillEntityAsync(entity);
        }

        protected override async Task<Core.Posts.Entities.Post> FillEntityAsync(Core.Posts.Entities.Post entity)
        {
            entity = await base.FillEntityAsync(entity);
            return entity;
        }

        public PostRequestItem(LinkGenerator linkGenerator, SitesRepository sitesRepository) : base(linkGenerator, sitesRepository)
        {
        }
    }

    public class Post : PostRequestItem, IContentResponseRestModel<Core.Posts.Entities.Post>
    {
        public IUser Author { get; set; }
        public int AuthorId { get; set; }

        protected override async Task ParseEntityAsync(Core.Posts.Entities.Post entity)
        {
            await base.ParseEntityAsync(entity);
            Blocks = entity.Blocks != null
                ? entity.Blocks.OrderBy(b => b.Position).Select(ContentBlock.Create).ToList()
                : new List<ContentBlock>();
            AuthorId = entity.AuthorId;
            Author = entity.Author;
        }


        public async Task SetEntityAsync(Core.Posts.Entities.Post entity)
        {
            await ParseEntityAsync(entity);
        }

        public Post(LinkGenerator linkGenerator, SitesRepository sitesRepository) : base(linkGenerator, sitesRepository)
        {
        }
    }
}
