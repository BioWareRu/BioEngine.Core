using BioEngine.Core.Properties;

namespace BioEngine.Extra.Twitter
{
    [PropertiesSet("Настройки публикации в Twitter", IsEditable = true)]
    public class TwitterSitePropertiesSet : PropertiesSet
    {
        [PropertiesElement("Включено?", PropertyElementType.Checkbox)]
        public bool IsEnabled { get; set; }

        [PropertiesElement("Twitter handle")] public string Handle { get; set; } = "";

        [PropertiesElement("Consumer Key")] public string ConsumerKey { get; set; } = "";

        [PropertiesElement("Consumer Secret", PropertyElementType.PasswordString)]
        public string ConsumerSecret { get; set; } = "";

        [PropertiesElement("Access token")] public string AccessToken { get; set; } = "";

        [PropertiesElement("Access token secret", PropertyElementType.PasswordString)]
        public string AccessTokenSecret { get; set; } = "";
    }
}
