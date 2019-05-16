using System;
using System.Collections.Generic;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public interface IMainSiteSelectionPolicy
    {
        Guid Get(ISiteEntity siteEntity);
        Guid Get(ISectionEntity contentEntity, IEnumerable<Section> sections);
    }
}
