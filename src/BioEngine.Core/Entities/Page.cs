using System.ComponentModel.DataAnnotations;

namespace BioEngine.Core.Entities
{
    public class Page : BaseSiteEntity
    {
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        [Required] public string Text { get; set; }
    }
}
