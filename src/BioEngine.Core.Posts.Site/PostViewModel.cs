using System;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Site.Model;

namespace BioEngine.Core.Posts.Site
{
    public class PostViewModel<TUserPk> : EntityViewModel<Post<TUserPk>>
    {
        public int CommentsCount { get; }
        public Uri CommentsUri { get; }

        public PostViewModel(PageViewModelContext context, Post<TUserPk> entity, int commentsCount,
            Uri commentsUri,
            ContentEntityViewMode mode = ContentEntityViewMode.List) :
            base(context, entity, mode)
        {
            CommentsCount = commentsCount;
            CommentsUri = commentsUri;
        }
    }
}
