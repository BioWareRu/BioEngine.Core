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
            RegisterRepositories(typeof(AdsModule).Assembly, serviceCollection, entitiesManager);
            entitiesManager.ConfigureDbContext(modelBuilder =>
            {
                modelBuilder.Entity<Ad>().HasMany(contentItem => contentItem.Blocks).WithOne()
                    .HasForeignKey(c => c.ContentId);
            });
        }
    }
}
