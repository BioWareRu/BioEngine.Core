using System.Threading.Tasks;
using BioEngine.Core.Api.Models;
using BioEngine.Core.Properties;

namespace BioEngine.Core.Api.Entities
{
    public class Tag : RestModel<Core.Entities.Tag>, IRequestRestModel<Core.Entities.Tag>,
        IResponseRestModel<Core.Entities.Tag>
    {
        public async Task<Core.Entities.Tag> GetEntityAsync(Core.Entities.Tag entity)
        {
            entity = await FillEntityAsync(entity);
            return entity;
        }

        public async Task SetEntityAsync(Core.Entities.Tag entity)
        {
            await ParseEntityAsync(entity);
        }

        public Tag(PropertiesProvider propertiesProvider) : base(propertiesProvider)
        {
        }
    }
}
