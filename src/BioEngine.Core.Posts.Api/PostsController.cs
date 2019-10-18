using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Api;
using BioEngine.Core.Api.Entities;
using BioEngine.Core.Entities;
using BioEngine.Core.Posts.Api.Entities;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Posts.Api
{
    [Authorize(Policy = PostsPolicies.Posts)]
    public abstract class
        ApiPostsController<TUserPk> : ContentEntityController<Posts.Entities.Post<TUserPk>, PostsRepository<TUserPk>,
            Post<TUserPk>, PostRequestItem<TUserPk>>
    {
        private readonly IUserDataProvider<TUserPk> _userDataProvider;
        private readonly ICurrentUserProvider<TUserPk> _currentUserProvider;

        protected ApiPostsController(
            BaseControllerContext<Posts.Entities.Post<TUserPk>, PostsRepository<TUserPk>> context,
            ContentBlocksRepository blocksRepository, IUserDataProvider<TUserPk> userDataProvider,
            ICurrentUserProvider<TUserPk> currentUserProvider) : base(context,
            blocksRepository)
        {
            _userDataProvider = userDataProvider;
            _currentUserProvider = currentUserProvider;
        }

        public override async Task<ActionResult<StorageItem>> UploadAsync(string name)
        {
            var file = await GetBodyAsFileAsync();
            return await Storage.SaveFileAsync(file, name,
                $"posts/{DateTimeOffset.UtcNow.Year.ToString()}/{DateTimeOffset.UtcNow.Month.ToString()}");
        }

        [HttpGet("{postId}/versions")]
        [Authorize(Policy = PostsPolicies.PostsEdit)]
        public async Task<ActionResult<List<ContentItemVersionInfo<TUserPk>>>> GetVersionsAsync(Guid postId)
        {
            var versions = await Repository.GetVersionsAsync(postId);
            var userIds =
                await _userDataProvider.GetDataAsync(versions.Select(v => v.ChangeAuthorId).Distinct().ToArray());
            return Ok(versions.Select(v =>
                    new ContentItemVersionInfo<TUserPk>(v.Id, v.DateAdded,
                        userIds.FirstOrDefault(u => u.Id.Equals(v.ChangeAuthorId))))
                .ToList());
        }

        [HttpGet("{postId}/versions/{versionId}")]
        [Authorize(Policy = PostsPolicies.PostsEdit)]
        public async Task<ActionResult<Post<TUserPk>>> GetVersionAsync(Guid postId, Guid versionId)
        {
            var version = await Repository.GetVersionAsync(postId, versionId);
            if (version == null)
            {
                return NotFound();
            }

            var post = version.GetContent<Posts.Entities.Post<TUserPk>>();
            return Ok(await MapRestModelAsync(post));
        }

        [Authorize(Policy = PostsPolicies.PostsAdd)]
        public override Task<ActionResult<Post<TUserPk>>> NewAsync()
        {
            return base.NewAsync();
        }

        [Authorize(Policy = PostsPolicies.PostsAdd)]
        public override Task<ActionResult<Post<TUserPk>>> AddAsync(PostRequestItem<TUserPk> item)
        {
            return base.AddAsync(item);
        }

        [Authorize(Policy = PostsPolicies.PostsEdit)]
        public override Task<ActionResult<Post<TUserPk>>> UpdateAsync(Guid id, PostRequestItem<TUserPk> item)
        {
            return base.UpdateAsync(id, item);
        }

        [Authorize(Policy = PostsPolicies.PostsDelete)]
        public override Task<ActionResult<Post<TUserPk>>> DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

        [Authorize(Policy = PostsPolicies.PostsPublish)]
        public override Task<ActionResult<Post<TUserPk>>> PublishAsync(Guid id)
        {
            return base.PublishAsync(id);
        }

        [Authorize(Policy = PostsPolicies.PostsPublish)]
        public override Task<ActionResult<Post<TUserPk>>> HideAsync(Guid id)
        {
            return base.HideAsync(id);
        }

        protected override async Task<Posts.Entities.Post<TUserPk>> MapDomainModelAsync(
            PostRequestItem<TUserPk> restModel, Posts.Entities.Post<TUserPk> domainModel = null)
        {
            domainModel = await base.MapDomainModelAsync(restModel, domainModel);
            if (domainModel.AuthorId == null)
            {
                domainModel.AuthorId = _currentUserProvider.CurrentUser.Id;
            }

            return domainModel;
        }
    }
}
