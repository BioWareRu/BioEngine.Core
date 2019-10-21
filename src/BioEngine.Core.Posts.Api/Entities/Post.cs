using System.Threading.Tasks;
using BioEngine.Core.Api.Models;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Posts.Api.Entities
{
    public class PostRequestItem<TUserPk> : SectionEntityRestModel<Core.Posts.Entities.Post<TUserPk>>,
        IContentRequestRestModel<Core.Posts.Entities.Post<TUserPk>>
    {
        public async Task<Core.Posts.Entities.Post<TUserPk>> GetEntityAsync(Core.Posts.Entities.Post<TUserPk> entity)
        {
            return await FillEntityAsync(entity);
        }

        protected override async Task<Core.Posts.Entities.Post<TUserPk>> FillEntityAsync(
            Core.Posts.Entities.Post<TUserPk> entity)
        {
            entity = await base.FillEntityAsync(entity);
            return entity;
        }

        public PostRequestItem(LinkGenerator linkGenerator, SitesRepository sitesRepository,
            PropertiesProvider propertiesProvider) : base(linkGenerator,
            sitesRepository, propertiesProvider)
        {
        }
    }

    public class Post<TUserPk> : PostRequestItem<TUserPk>, IContentResponseRestModel<Core.Posts.Entities.Post<TUserPk>>
    {
        public IUser<TUserPk> Author { get; set; }
        public TUserPk AuthorId { get; set; }

        protected override async Task ParseEntityAsync(Core.Posts.Entities.Post<TUserPk> entity)
        {
            await base.ParseEntityAsync(entity);
            AuthorId = entity.AuthorId;
            Author = entity.Author;
        }


        public async Task SetEntityAsync(Core.Posts.Entities.Post<TUserPk> entity)
        {
            await ParseEntityAsync(entity);
        }

        public Post(LinkGenerator linkGenerator, SitesRepository sitesRepository, PropertiesProvider propertiesProvider)
            : base(linkGenerator, sitesRepository, propertiesProvider)
        {
        }
    }
}
