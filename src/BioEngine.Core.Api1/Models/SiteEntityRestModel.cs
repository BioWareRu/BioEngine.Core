using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Api.Models
{
    public abstract class SiteEntityRestModel<TEntity> : RestModel<TEntity>
        where TEntity : class, ISiteEntity, IEntity
    {
        public Guid[] SiteIds { get; set; }

        protected override async Task ParseEntityAsync(TEntity entity)
        {
            await base.ParseEntityAsync(entity);
            SiteIds = entity.SiteIds;
        }

        protected override async Task<TEntity> FillEntityAsync(TEntity entity)
        {
            entity = await base.FillEntityAsync(entity);
            entity.SiteIds = SiteIds;
            return entity;
        }
    }
}
