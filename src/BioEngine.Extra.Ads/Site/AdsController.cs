using System;
using System.Threading.Tasks;
using BioEngine.Extra.Ads.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Extra.Ads.Site
{
    public abstract class AdsSiteController : Controller
    {
        private readonly AdsRepository _adsRepository;

        protected AdsSiteController(AdsRepository adsRepository)
        {
            _adsRepository = adsRepository;
        }

        [HttpGet("go/{adId}.html")]
        public virtual async Task<ActionResult> RedirectAsync(Guid adId)
        {
            var ad = await _adsRepository.GetByIdAsync(adId);
            if (ad == null)
            {
                return NotFound();
            }

            return Redirect(ad.Url);
        }
    }
}
