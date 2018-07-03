﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Repository
{
    public abstract class SiteEntityRepository<T, TId> : BioRepository<T, TId>
        where T : class, IEntity<TId>, ISiteEntity
    {
        private readonly SitesRepository _sitesRepository;

        protected SiteEntityRepository(BioRepositoryContext<T, TId> repositoryContext, SitesRepository sitesRepository)
            : base(repositoryContext)
        {
            _sitesRepository = sitesRepository;
        }

        protected override IQueryable<T> ApplyContext(IQueryable<T> query, QueryContext<T, TId> queryContext)
        {
            if ((queryContext?.SiteId).HasValue)
            {
                query = query.Where(e => e.SiteIds.Contains(queryContext.SiteId.Value));
            }

            return base.ApplyContext(query, queryContext);
        }

        public override async Task<(List<T> items, int itemsCount)> GetAll(QueryContext<T, TId> queryContext = null,
            Func<IQueryable<T>, IQueryable<T>> addConditionsCallback = null)
        {
            var entities = await base.GetAll(queryContext, addConditionsCallback);

            return entities;
        }
    }
}