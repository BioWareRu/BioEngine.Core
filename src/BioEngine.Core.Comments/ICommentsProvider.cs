using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Comments
{
    public interface ICommentsProvider
    {
        Task<Dictionary<Guid, (int count, Uri? uri)>> GetCommentsDataAsync(ContentItem[] entities);
        Task<Dictionary<Guid, Uri?>> GetCommentsUrlAsync(ContentItem[] entities);
        Task<IEnumerable<BaseComment>> GetLastCommentsAsync(Site site, int count);

        Task<List<(ContentItem entity, int commentsCount)>> GetMostCommentedAsync(Site site, int count,
            TimeSpan period);
        
        Task<BaseComment> AddCommentAsync(ContentItem entity, string text, string authorId, Guid? replyTo = null);
        Task<BaseComment> UpdateCommentAsync(ContentItem entity, Guid commentId, string text);
        Task<BaseComment> DeleteCommentAsync(ContentItem entity, Guid commentId);
        Task<IEnumerable<BaseComment>> GetCommentsAsync(ContentItem entity);
        Task<BaseComment> GetCommentByIdAsync(ContentItem entity, Guid commentId);
    }
}
