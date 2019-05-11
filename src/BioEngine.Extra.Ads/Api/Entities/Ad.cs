using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BioEngine.Core.API.Models;
using BioEngine.Core.Entities;

namespace BioEngine.Extra.Ads.Api.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class Ad : SiteEntityRestModel<Ads.Entities.Ad>, IRequestRestModel<Ads.Entities.Ad>,
        IResponseRestModel<Ads.Entities.Ad>
    {
        [Required] public StorageItem Picture { get; set; }

        public async Task<Ads.Entities.Ad> GetEntityAsync(Ads.Entities.Ad entity)
        {
            entity = await FillEntityAsync(entity);
            entity.Picture = Picture;
            return entity;
        }

        public async Task SetEntityAsync(Ads.Entities.Ad entity)
        {
            await ParseEntityAsync(entity);
            Picture = entity.Picture;
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
