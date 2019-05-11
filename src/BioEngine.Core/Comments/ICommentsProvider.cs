using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Comments
{
    public interface ICommentsProvider
    {
        Task<Dictionary<Guid, (int count, Uri? uri)>> GetCommentsDataAsync(IContentEntity[] entities);
        Task<Dictionary<Guid, Uri?>> GetCommentsUrlAsync(IContentEntity[] entities);
        Task<IEnumerable<BaseComment>> GetLastCommentsAsync(Site site, int count);

        Task<List<(IContentEntity entity, int commentsCount)>> GetMostCommentedAsync(Site site, int count,
            TimeSpan period);
    }
}
