using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Site.Model
{
    public class PageViewModelContext
    {
        public PageViewModelContext(LinkGenerator linkGenerator, PropertiesProvider propertiesProvider,
            Entities.Site site, Section section = null)
        {
            LinkGenerator = linkGenerator;
            PropertiesProvider = propertiesProvider;
            Site = site;
            Section = section;
        }

        public LinkGenerator LinkGenerator { get; }
        public PropertiesProvider PropertiesProvider { get; }
        public Entities.Site Site { get; }
        public Section Section { get; }
    }
}