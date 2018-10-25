namespace BioEngine.Core.Properties
{
    public class PropertiesValue
    {
        public PropertiesValue(int? siteId, PropertiesSet value)
        {
            SiteId = siteId;
            Value = value;
        }

        public int? SiteId { get; set; }
        public PropertiesSet Value { get; set; }
    }
}