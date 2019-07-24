using BioEngine.Core.Api.Models;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Api.Entities
{
    public class Section : ResponseSectionRestModel<Core.Entities.Section>
    {
        public Section(LinkGenerator linkGenerator, SitesRepository sitesRepository,
            PropertiesProvider propertiesProvider) : base(linkGenerator,
            sitesRepository, propertiesProvider)
        {
        }
    }
}
