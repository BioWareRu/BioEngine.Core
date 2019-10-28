using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Social
{
    public abstract class BaseContentPublisher<TConfig, TPublishRecord> : IContentPublisher<TConfig>
        where TConfig : IContentPublisherConfig where TPublishRecord : BasePublishRecord, new()
    {
        protected ILogger<BaseContentPublisher<TConfig, TPublishRecord>> Logger { get; }
        private readonly BioContext _dbContext;

        protected BaseContentPublisher(BioContext dbContext,
            ILogger<BaseContentPublisher<TConfig, TPublishRecord>> logger)
        {
            Logger = logger;
            _dbContext = dbContext;
        }

        public virtual async Task<bool> PublishAsync(IContentItem entity, TConfig config, bool needUpdate,
            Site? site = null)
        {
            try
            {
                var isNew = false;
                var record = await GetRecordAsync(entity, site);
                if (record != null && !needUpdate)
                {
                    return true;
                }

                if (record == null)
                {
                    isNew = true;
                    record = new TPublishRecord
                    {
                        Id = Guid.NewGuid(),
                        ContentId = entity.Id,
                        Type = entity.GetKey(),
                        SiteIds = site != null ? new[] {site.Id} : entity.SiteIds
                    };
                }

                await DoPublishAsync(record, entity, site, config);
                if (isNew)
                {
                    _dbContext.Set<TPublishRecord>().Add(record);
                }
                else
                {
                    _dbContext.Set<TPublishRecord>().Update(record);
                }

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

        public virtual async Task<bool> DeleteAsync(IContentItem entity, TConfig config, Site? site = null)
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

                _dbContext.Set<TPublishRecord>().Remove(record);
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        protected virtual async Task<TPublishRecord> GetRecordAsync(IContentItem entity, Site? site = null)
        {
            return await _dbContext.Set<TPublishRecord>()
                .FirstOrDefaultAsync(r =>
                    r.Type == entity.GetKey() && r.ContentId == entity.Id
                                              && (site == null || r.SiteIds.Contains(site.Id)));
        }

        protected async Task<IEnumerable<TPublishRecord>> GetRecordsAsync(IContentItem entity)
        {
            return await _dbContext.Set<TPublishRecord>()
                .Where(r =>
                    r.Type == entity.GetKey() && r.ContentId == entity.Id)
                .ToListAsync();
        }

        protected abstract Task<TPublishRecord> DoPublishAsync(TPublishRecord record, IContentItem entity, Site site,
            TConfig config);

        protected abstract Task<bool> DoDeleteAsync(TPublishRecord record, TConfig config);
    }
}
