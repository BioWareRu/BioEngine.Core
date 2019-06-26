using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Api.Entities;
using BioEngine.Core.Api.Models;

namespace BioEngine.Core.Pages.Api.Entities
{
    public class Page : SiteEntityRestModel<global::BioEngine.Core.Pages.Entities.Page>,
        IContentRequestRestModel<global::BioEngine.Core.Pages.Entities.Page>,
        IContentResponseRestModel<global::BioEngine.Core.Pages.Entities.Page>
    {
        public List<ContentBlock> Blocks { get; set; }

        public async Task<global::BioEngine.Core.Pages.Entities.Page> GetEntityAsync(global::BioEngine.Core.Pages.Entities.Page entity)
        {
            entity = await FillEntityAsync(entity);
            return entity;
        }

        public async Task SetEntityAsync(global::BioEngine.Core.Pages.Entities.Page entity)
        {
            await ParseEntityAsync(entity);
            Blocks = entity.Blocks?.Select(ContentBlock.Create).ToList();
        }
    }
}
