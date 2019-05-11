using System;
using System.Threading.Tasks;
using BioEngine.Core.API;
using BioEngine.Core.Entities;
using BioEngine.Core.Web;
using BioEngine.Extra.Ads.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Extra.Ads.Api
{
    public abstract class AdsApiController : ResponseRequestRestController<Ad, Entities.Ad>
    {
        protected AdsApiController(BaseControllerContext<Ad> context) : base(context)
        {
        }
        
        public override async Task<ActionResult<StorageItem>> UploadAsync(string name)
        {
            var file = await GetBodyAsFileAsync();
            return await Storage.SaveFileAsync(file, name,
                $"ads/{DateTimeOffset.UtcNow.Year.ToString()}/{DateTimeOffset.UtcNow.Month.ToString()}");
        }
    }
}
