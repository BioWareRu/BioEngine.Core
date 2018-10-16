namespace BioEngine.Core.Settings
{
    public class SettingsValue
    {
        public SettingsValue(int? siteId, SettingsBase value)
        {
            SiteId = siteId;
            Value = value;
        }

        public int? SiteId { get; set; }
        public SettingsBase Value { get; set; }
    }
}