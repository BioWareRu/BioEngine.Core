using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    public class Menu : BaseSingleSiteEntity
    {
        [NotMapped] public override string Url { get; set; } = "";
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }

    public class MenuItem
    {
        public string Label { get; set; } = "";
        public string Url { get; set; } = "";
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }
}
