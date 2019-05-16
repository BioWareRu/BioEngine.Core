﻿using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    public class PagesRepository : SiteEntityRepository<Page>
    {
        protected override IQueryable<Page> GetBaseQuery(QueryContext<Page>? queryContext = null)
        {
            return ApplyContext(DbContext.Set<Page>().Include(p => p.Blocks), queryContext);
        }

        public PagesRepository(BioRepositoryContext<Page> repositoryContext,
            IMainSiteSelectionPolicy mainSiteSelectionPolicy) : base(repositoryContext, mainSiteSelectionPolicy)
        {
        }
    }
}
