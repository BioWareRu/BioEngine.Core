using System;
using System.Threading.Tasks;
using BioEngine.Core.Site;
using BioEngine.Core.Web;
using BioEngine.Extra.Ads.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Extra.Ads.Site
{
    public abstract class AdsSiteController : BaseSiteController
    {
        private readonly AdsRepository _adsRepository;

        protected AdsSiteController(BaseControllerContext context, AdsRepository adsRepository) : base(context)
        {
            _adsRepository = adsRepository;
        }

        public virtual async Task<IActionResult> RedirectAsync(Guid adId)
        {
            var ad = await _adsRepository.GetByIdAsync(adId);
            if (ad == null)
            {
                return PageNotFound();
            }

            return Redirect(ad.Url);
        }
    }
}
