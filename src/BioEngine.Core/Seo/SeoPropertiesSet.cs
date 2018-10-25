using BioEngine.Core.Properties;

namespace BioEngine.Core.Seo
{
    [PropertiesSet(Name = "Seo", IsEditable = true)]
    public class SeoPropertiesSet : PropertiesSet
    {
        [PropertiesElement(Name = "Описание", Type = PropertyElementType.LongString)]
        public string Description { get; set; }

        [PropertiesElement(Name = "Ключевые слова", Type = PropertyElementType.String)]
        public string Keywords { get; set; }
    }
}