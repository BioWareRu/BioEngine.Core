using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Repository
{
    [UsedImplicitly]
    public class SectionsRepository : SiteEntityRepository<Section, int>
    {
        public SectionsRepository(BioRepositoryContext<Section, int> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
