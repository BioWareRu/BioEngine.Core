using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Api.Models;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Api.Entities
{
    public class Menu : SiteEntityRestModel<Core.Entities.Menu>, IRequestRestModel<Core.Entities.Menu>,
        IResponseRestModel<Core.Entities.Menu>
    {
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();

        public async Task<Core.Entities.Menu> GetEntityAsync(Core.Entities.Menu entity)
        {
            entity = await FillEntityAsync(entity);
            entity.Items = Items;
            return entity;
        }

        public async Task SetEntityAsync(Core.Entities.Menu entity)
        {
            await ParseEntityAsync(entity);
            Items = entity.Items;
        }

        public Menu(PropertiesProvider propertiesProvider) : base(propertiesProvider)
        {
        }
    }
}
