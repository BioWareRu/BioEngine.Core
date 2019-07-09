using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Api;
using BioEngine.Core.Api.Entities;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Api.Entities;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Post = BioEngine.Core.Posts.Entities.Post;

namespace BioEngine.Core.Posts.Api
{
    [Authorize(Policy = PostsPolicies.Posts)]
    public abstract class
        ApiPostsController : ContentEntityController<Post, PostsRepository, Entities.Post, PostRequestItem>
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

        public override async Task<ActionResult<StorageItem>> UploadAsync(string name)
        {
            var file = await GetBodyAsFileAsync();
            return await Storage.SaveFileAsync(file, name,
                $"posts/{DateTimeOffset.UtcNow.Year.ToString()}/{DateTimeOffset.UtcNow.Month.ToString()}");
        }

        [HttpGet("{postId}/versions")]
        [Authorize(Policy = PostsPolicies.PostsEdit)]
        public async Task<ActionResult<List<ContentItemVersionInfo>>> GetVersionsAsync(Guid postId)
        {
            var versions = await Repository.GetVersionsAsync(postId);
            var userIds =
                await _userDataProvider.GetDataAsync(versions.Select(v => v.ChangeAuthorId).Distinct().ToArray());
            return Ok(versions.Select(v =>
                    new ContentItemVersionInfo(v.Id, v.DateAdded,
                        userIds.FirstOrDefault(u => u.Id == v.ChangeAuthorId)))
                .ToList());
        }

        [HttpGet("{postId}/versions/{versionId}")]
        [Authorize(Policy = PostsPolicies.PostsEdit)]
        public async Task<ActionResult<Entities.Post>> GetVersionAsync(Guid postId, Guid versionId)
        {
            var version = await Repository.GetVersionAsync(postId, versionId);
            if (version == null)
            {
                return NotFound();
            }

            var post = version.GetContent<Post, PostData>();
            return Ok(await MapRestModelAsync(post));
        }

        [Authorize(Policy = PostsPolicies.PostsAdd)]
        public override Task<ActionResult<Entities.Post>> NewAsync()
        {
            return base.NewAsync();
        }

        [Authorize(Policy = PostsPolicies.PostsAdd)]
        public override Task<ActionResult<Entities.Post>> AddAsync(PostRequestItem item)
        {
            return base.AddAsync(item);
        }

        [Authorize(Policy = PostsPolicies.PostsEdit)]
        public override Task<ActionResult<Entities.Post>> UpdateAsync(Guid id, PostRequestItem item)
        {
            return base.UpdateAsync(id, item);
        }

        [Authorize(Policy = PostsPolicies.PostsDelete)]
        public override Task<ActionResult<Entities.Post>> DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

        [Authorize(Policy = PostsPolicies.PostsPublish)]
        public override Task<ActionResult<Entities.Post>> PublishAsync(Guid id)
        {
            return base.PublishAsync(id);
        }

        [Authorize(Policy = PostsPolicies.PostsPublish)]
        public override Task<ActionResult<Entities.Post>> HideAsync(Guid id)
        {
            return base.HideAsync(id);
        }
    }
}
