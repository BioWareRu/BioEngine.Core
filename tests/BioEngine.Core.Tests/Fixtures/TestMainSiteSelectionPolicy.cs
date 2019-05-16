using System;
using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Tests.Fixtures
{
    public class TestMainSiteSelectionPolicy : IMainSiteSelectionPolicy
    {
        public Guid Get(ISiteEntity siteEntity)
        {
            return siteEntity.SiteIds.First();
        }

        public Guid Get(ISectionEntity contentEntity, IEnumerable<Section> sections)
        {
            return sections.First().SiteIds.First();
        }
    }
}
