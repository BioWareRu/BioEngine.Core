using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BioEngine.Core.DB;

namespace BioEngine.Core.Entities
{
    [Entity("menu")]
    public class Menu : BaseSiteEntity
    {
        [Required] public string Title { get; set; }
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }

    public class MenuItem
    {
        public string Label { get; set; } = "";
        public string Url { get; set; } = "";
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }
}
