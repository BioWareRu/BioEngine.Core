using System;

namespace BioEngine.Core.Properties
{
    public class PropertiesValue
    {
        public PropertiesValue(Guid? siteId, PropertiesSet value)
        {
            SiteId = siteId;
            Value = value;
        }

        public Guid? SiteId { get; set; }
        public PropertiesSet Value { get; set; }
    }
}
