using HtmlAgilityPack;

namespace BioEngine.Core.Site.Helpers
{
    public static class HtmlHelper
    {
        public static string GetDescriptionFromHtml(string html)
        {
            if (html == null)
            {
                return "";
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            return HtmlEntity.DeEntitize(htmlDoc.DocumentNode.InnerText.Trim('\r', '\n')).Trim();
        }
    }
}
