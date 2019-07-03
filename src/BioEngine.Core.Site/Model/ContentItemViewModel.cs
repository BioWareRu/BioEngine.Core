using System;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Site.Model
{
    public class ContentItemViewModel : EntityViewModel<ContentItem>
    {
        public int CommentsCount { get; }
        public Uri CommentsUri { get; }

        public ContentItemViewModel(PageViewModelContext context, ContentItem entity, int commentsCount,
            Uri commentsUri,
            ContentEntityViewMode mode = ContentEntityViewMode.List) :
            base(context, entity, mode)
        {
            CommentsCount = commentsCount;
            CommentsUri = commentsUri;
        }
    }
}