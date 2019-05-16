using BioEngine.Core.Repository;

namespace BioEngine.Extra.Ads.Entities
{
    public class AdsRepository : SiteEntityRepository<Ad>
    {
        public AdsRepository(BioRepositoryContext<Ad> repositoryContext,
            IMainSiteSelectionPolicy mainSiteSelectionPolicy) : base(repositoryContext, mainSiteSelectionPolicy)
        {
        }
    }
}
