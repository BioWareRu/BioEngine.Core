using System;
using System.Threading.Tasks;
using BioEngine.Core.Api;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;
using BioEngine.Extra.Ads.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Extra.Ads.Api
{
    [Authorize(Policy = AdsPolicies.Ads)]
    public abstract class
        AdsApiController : ContentEntityController<Ad, AdsRepository, Entities.Ad, Entities.Ad>
    {
        protected AdsApiController(BaseControllerContext<Ad, AdsRepository> context,
            BioEntitiesManager entitiesManager, ContentBlocksRepository blocksRepository) : base(context,
            entitiesManager, blocksRepository)
        {
        }

        public override async Task<ActionResult<StorageItem>> UploadAsync(string name)
        {
            var file = await GetBodyAsFileAsync();
            return await Storage.SaveFileAsync(file, name,
                $"ads/{DateTimeOffset.UtcNow.Year.ToString()}/{DateTimeOffset.UtcNow.Month.ToString()}");
        }

        [Authorize(Policy = AdsPolicies.AdsAdd)]
        public override Task<ActionResult<Entities.Ad>> NewAsync()
        {
            return base.NewAsync();
        }

        [Authorize(Policy = AdsPolicies.AdsAdd)]
        public override Task<ActionResult<Entities.Ad>> AddAsync(Entities.Ad item)
        {
            return base.AddAsync(item);
        }

        [Authorize(Policy = AdsPolicies.AdsEdit)]
        public override Task<ActionResult<Entities.Ad>> UpdateAsync(Guid id, Entities.Ad item)
        {
            return base.UpdateAsync(id, item);
        }

        [Authorize(Policy = AdsPolicies.AdsDelete)]
        public override Task<ActionResult<Entities.Ad>> DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

        [Authorize(Policy = AdsPolicies.AdsPublish)]
        public override Task<ActionResult<Entities.Ad>> PublishAsync(Guid id)
        {
            return base.PublishAsync(id);
        }

        [Authorize(Policy = AdsPolicies.AdsPublish)]
        public override Task<ActionResult<Entities.Ad>> HideAsync(Guid id)
        {
            return base.HideAsync(id);
        }
    }
}
