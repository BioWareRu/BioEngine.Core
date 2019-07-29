using System.Threading.Tasks;
using BioEngine.Core.Api.Models;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Api.Entities
{
    public class Site : RestModel<Core.Entities.Site>, IRequestRestModel<Core.Entities.Site>,
        IResponseRestModel<Core.Entities.Site>
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public async Task<Core.Entities.Site> GetEntityAsync(Core.Entities.Site entity)
        {
            entity = await FillEntityAsync(entity);
            entity.Url = Url;
            entity.Title = Title;
            return entity;
        }

        public async Task SetEntityAsync(Core.Entities.Site entity)
        {
            await ParseEntityAsync(entity);
            Url = entity.Url;
            Title = entity.Title;
        }

        public Site(PropertiesProvider propertiesProvider) : base(propertiesProvider)
        {
        }
    }
}
