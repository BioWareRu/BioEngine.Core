using HtmlAgilityPack;

namespace BioEngine.Core.Site.Helpers
{
    public static class HtmlHelper
    {
        public static string GetDescriptionFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            return HtmlEntity.DeEntitize(htmlDoc.DocumentNode.InnerText.Trim('\r', '\n')).Trim();
        }
    }
}
