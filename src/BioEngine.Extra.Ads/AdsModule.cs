using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Extra.Ads.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.Ads
{
    public abstract class AdsModule : BaseBioEngineModule
    {
        public override void ConfigureEntities(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager)
        {
            base.ConfigureEntities(serviceCollection, entitiesManager);
            RegisterRepositories(typeof(Ad).Assembly, serviceCollection, entitiesManager);
        }
    }
}
