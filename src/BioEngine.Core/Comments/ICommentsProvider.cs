using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Comments
{
    public interface ICommentsProvider
    {
        Task<int> GetCommentsCountAsync(IContentEntity entity);
        Task<Dictionary<Guid, int>> GetCommentsCountAsync(IEnumerable<IContentEntity> entities);
        Task<Uri> GetCommentsUrlAsync(IContentEntity entity);
        Task<IEnumerable<BaseComment>> GetLastCommentsAsync(Site site, int count);
        Task<List<(IContentEntity entity, int commentsCount)>> GetMostCommentedAsync(Site site, int count, TimeSpan period);
    }
}
