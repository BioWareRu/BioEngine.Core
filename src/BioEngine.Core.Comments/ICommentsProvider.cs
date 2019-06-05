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
    }
}
