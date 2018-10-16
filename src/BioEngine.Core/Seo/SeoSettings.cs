using BioEngine.Core.Settings;

namespace BioEngine.Core.Seo
{
    [SettingsClass(Name = "Seo", IsEditable = true)]
    public class SeoSettings : SettingsBase
    {
        [SettingsProperty(Name = "Описание", Type = SettingType.LongString)]
        public string Description { get; set; }

        [SettingsProperty(Name = "Ключевые слова", Type = SettingType.String)]
        public string Keywords { get; set; }
    }
}