using System.Threading.Tasks;
using BioEngine.Core.Api.Models;
using BioEngine.Core.Repository;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Pages.Api.Entities
{
    public class Page : ContentEntityRestModel<global::BioEngine.Core.Pages.Entities.Page>,
        IContentRequestRestModel<global::BioEngine.Core.Pages.Entities.Page>,
        IContentResponseRestModel<global::BioEngine.Core.Pages.Entities.Page>
    {
        public async Task<global::BioEngine.Core.Pages.Entities.Page> GetEntityAsync(
            global::BioEngine.Core.Pages.Entities.Page entity)
        {
            entity = await FillEntityAsync(entity);
            return entity;
        }

        public async Task SetEntityAsync(global::BioEngine.Core.Pages.Entities.Page entity)
        {
            await ParseEntityAsync(entity);
        }

        public Page(LinkGenerator linkGenerator, SitesRepository sitesRepository) : base(linkGenerator, sitesRepository)
        {
        }
    }
}
