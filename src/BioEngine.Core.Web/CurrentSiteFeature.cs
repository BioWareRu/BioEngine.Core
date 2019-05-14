namespace BioEngine.Core.Web
{
    public class CurrentSiteFeature
    {
        public CurrentSiteFeature(Entities.Site site)
        {
            Site = site;
        }

        public Entities.Site Site { get; }
    }
}
