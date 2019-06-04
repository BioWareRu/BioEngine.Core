using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.API;
using BioEngine.Core.API.Entities;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Posts.Api
{
    public abstract class
        ApiPostsController : ContentEntityController<Post, PostsRepository, Entities.Post, Entities.PostRequestItem>
    {
        private readonly IUserDataProvider _userDataProvider;

        protected ApiPostsController(
            BaseControllerContext<Post, PostsRepository> context,
            BioEntitiesManager entitiesManager,
            ContentBlocksRepository blocksRepository, IUserDataProvider userDataProvider) : base(context,
            entitiesManager, blocksRepository)
        {
            _userDataProvider = userDataProvider;
        }

        protected override async Task<Post> MapDomainModelAsync(Entities.PostRequestItem restModel,
            Post domainModel = null)
        {
            domainModel = await base.MapDomainModelAsync(restModel, domainModel);
            if (domainModel.AuthorId == 0)
            {
                domainModel.AuthorId = CurrentUser.Id;
            }

            return domainModel;
        }

        public override async Task<ActionResult<StorageItem>> UploadAsync(string name)
        {
            var file = await GetBodyAsFileAsync();
            return await Storage.SaveFileAsync(file, name,
                $"posts/{DateTimeOffset.UtcNow.Year.ToString()}/{DateTimeOffset.UtcNow.Month.ToString()}");
        }

        [HttpGet("{postId}/versions")]
        public async Task<ActionResult<List<ContentItemVersionInfo>>> GetVersionsAsync(Guid postId)
        {
            var versions = await Repository.GetVersionsAsync(postId);
            var userIds =
                await _userDataProvider.GetDataAsync(versions.Select(v => v.ChangeAuthorId).Distinct().ToArray());
            return Ok(versions.Select(v =>
                    new ContentItemVersionInfo(v.Id, v.DateAdded, userIds.FirstOrDefault(u => u.Id == v.ChangeAuthorId)))
                .ToList());
        }

        [HttpGet("{postId}/versions/{versionId}")]
        public async Task<ActionResult<Entities.Post>> GetVersionsAsync(Guid postId, Guid versionId)
        {
            var version = await Repository.GetVersionAsync(postId, versionId);
            if (version == null)
            {
                return NotFound();
            }

            var post = version.GetContent<Post, PostData>();
            return Ok(await MapRestModelAsync(post));
        }
    }
}
