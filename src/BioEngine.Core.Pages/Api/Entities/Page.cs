using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.API.Entities;
using BioEngine.Core.API.Models;

namespace BioEngine.Core.Pages.Api.Entities
{
    public class Page : SiteEntityRestModel<Core.Pages.Entities.Page>,
        IContentRequestRestModel<Core.Pages.Entities.Page>,
        IContentResponseRestModel<Core.Pages.Entities.Page>
    {
        public List<ContentBlock> Blocks { get; set; }

        public async Task<Core.Pages.Entities.Page> GetEntityAsync(Core.Pages.Entities.Page entity)
        {
            entity = await FillEntityAsync(entity);
            return entity;
        }

        public async Task SetEntityAsync(Core.Pages.Entities.Page entity)
        {
            await ParseEntityAsync(entity);
            Blocks = entity.Blocks?.Select(ContentBlock.Create).ToList();
        }
    }
}
