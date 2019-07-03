using System;

namespace BioEngine.Core.Site.Model
{
    public class PageMetaModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public Uri ImageUrl { get; set; }
        public Uri CurrentUrl { get; set; }
    }
}