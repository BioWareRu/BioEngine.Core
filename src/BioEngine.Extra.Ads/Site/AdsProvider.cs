using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Extra.Ads.Entities;

namespace BioEngine.Extra.Ads.Site
{
    public class AdsProvider
    {
        private readonly AdsRepository _adsRepository;
        private Stack<Ad>? _ads;
        private static readonly Random Rng = new Random();

        private static void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public AdsProvider(AdsRepository adsRepository)
        {
            _adsRepository = adsRepository;
            _ads = null;
        }

        private async Task<Stack<Ad>> GetAdsAsync(Core.Entities.Site site)
        {
            if (_ads != null) return _ads;

            var context = new QueryContext<Ad>
            {
                IncludeUnpublished = false
            };
            context.SetSite(site);
            var ads = await _adsRepository.GetAllAsync(context);
            Shuffle(ads.items);
            _ads = new Stack<Ad>(ads.items);
            return _ads;
        }

        public async Task<Ad?> NextAsync(Core.Entities.Site site)
        {
            var ads = await GetAdsAsync(site);
            return ads.Count > 0 ? ads.Pop() : null;
        }
    }
}
