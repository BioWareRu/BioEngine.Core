using System;

namespace BioEngine.Core.Search
{
    public class SearchModel
    {
        public Guid Id { get; set; }
        public int? AuthorId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public Guid[] SiteIds { get; set; }
        public Guid[] SectionIds { get; set; }
        public DateTimeOffset Date { get; set; }
        public string[] Tags { get; set; }
        public string Content { get; set; }
    }
}
