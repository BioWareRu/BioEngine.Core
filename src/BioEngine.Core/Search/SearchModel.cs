using System;

namespace BioEngine.Core.Search
{
    public class SearchModel
    {
        public Guid Id { get; set; }
        public int? AuthorId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public Guid[] SiteIds { get; set; } = new Guid[0];
        public Guid[] SectionIds { get; set; } = new Guid[0];
        public DateTimeOffset Date { get; set; }
        public string[] Tags { get; set; } = new string[0];
        public string Content { get; set; }

        public SearchModel(Guid id, string title, string url, string content, DateTimeOffset date)
        {
            Id = id;
            Title = title;
            Url = url;
            Content = content;
            Date = date;
        }
    }
}
