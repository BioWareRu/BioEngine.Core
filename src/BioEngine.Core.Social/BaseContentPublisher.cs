using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Social
{
    public abstract class BaseContentPublisher<TConfig, TPublishRecord> : IContentPublisher<TConfig>
        where TConfig : IContentPublisherConfig where TPublishRecord : BasePublishRecord
    {
        protected ILogger<IContentPublisher<TConfig>> Logger { get; }
        private readonly BioContext _dbContext;

        protected BaseContentPublisher(BioContext dbContext, ILogger<IContentPublisher<TConfig>> logger)
        {
            Logger = logger;
            _dbContext = dbContext;
        }

        public virtual async Task<bool> PublishAsync(ContentItem entity, Site site, TConfig config)
        {
            try
            {
                var record = await DoPublishAsync(entity, site, config);
                _dbContext.Add(record);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while publishing to {publisher}: {errorText}", GetType().FullName,
                    ex.ToString());
                return false;
            }
        }

        public virtual async Task<bool> DeleteAsync(ContentItem entity, TConfig config, Site site = null)
        {
            var records = (await GetRecordsAsync(entity)).ToArray();
            if (!records.Any())
            {
                return false;
            }

            foreach (var record in records)
            {
                if (site != null && !record.SiteIds.Contains(site.Id))
                {
                    continue;
                }

                var deleted = await DoDeleteAsync(record, config);
                if (!deleted)
                {
                    Logger.LogError("Can't delete content from {publisher}", GetType().FullName);
                    return false;
                }

                _dbContext.Remove(record);
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        protected async Task<TPublishRecord> GetRecordAsync(ContentItem entity, Site site = null)
        {
            return await _dbContext.Set<TPublishRecord>()
                .FirstOrDefaultAsync(r =>
                    r.Type == entity.GetType().FullName && r.ContentId == entity.Id &&
                    (site == null || r.SiteIds.Contains(site.Id)));
        }

        protected async Task<IEnumerable<TPublishRecord>> GetRecordsAsync(ContentItem entity)
        {
            return await _dbContext.Set<TPublishRecord>()
                .Where(r =>
                    r.Type == entity.GetType().FullName && r.ContentId == entity.Id)
                .ToListAsync();
        }

        protected abstract Task<TPublishRecord> DoPublishAsync(ContentItem entity, Site site, TConfig config);
        protected abstract Task<bool> DoDeleteAsync(TPublishRecord record, TConfig config);
    }
}
