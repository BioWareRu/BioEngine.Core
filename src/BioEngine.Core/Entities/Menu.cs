using System.Collections.Generic;

namespace BioEngine.Core.Entities
{
    public class Menu : BaseSiteEntity<int>
    {
        public string Title { get; set; }
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }

    public class MenuItem
    {
        public string Label { get; set; }
        public string Url { get; set; }
        public List<MenuItem> Items { get; set; }
    }
}