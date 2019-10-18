using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Comments
{
    public interface ICommentsProvider<TUserPk>
    {
        Task<Dictionary<Guid, (int count, Uri? uri)>> GetCommentsDataAsync(IContentItem[] entities, Site site);
        Task<Dictionary<Guid, Uri?>> GetCommentsUrlAsync(IContentItem[] entities, Site site);

        Task<IEnumerable<BaseComment<TUserPk>>> GetLastCommentsAsync<TContent>(Site site, int count)
            where TContent : class, IContentItem;

        Task<List<(TContent entity, int commentsCount)>> GetMostCommentedAsync<TContent>(Site site, int count,
            TimeSpan period) where TContent : class, IContentItem;

        Task<BaseComment<TUserPk>> AddCommentAsync(IContentItem entity, string text, string authorId,
            Guid? replyTo = null);

        Task<BaseComment<TUserPk>> UpdateCommentAsync(IContentItem entity, Guid commentId, string text);
        Task<BaseComment<TUserPk>> DeleteCommentAsync(IContentItem entity, Guid commentId);
        Task<IEnumerable<BaseComment<TUserPk>>> GetCommentsAsync(IContentItem entity, Site site);
        Task<BaseComment<TUserPk>> GetCommentByIdAsync(IContentItem entity, Guid commentId);
    }
}
