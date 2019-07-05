using System.Threading.Tasks;
using BioEngine.Core.Api.Models;
using BioEngine.Core.Repository;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Extra.Ads.Api.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class Ad : ContentEntityRestModel<Ads.Entities.Ad>, IContentRequestRestModel<Ads.Entities.Ad>,
        IContentResponseRestModel<Ads.Entities.Ad>
    {
        public async Task<Ads.Entities.Ad> GetEntityAsync(Ads.Entities.Ad entity)
        {
            entity = await FillEntityAsync(entity);
            return entity;
        }

        public async Task SetEntityAsync(Ads.Entities.Ad entity)
        {
            await ParseEntityAsync(entity);
        }

        public Ad(LinkGenerator linkGenerator, SitesRepository sitesRepository) : base(linkGenerator, sitesRepository)
        {
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
