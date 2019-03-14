using System.ComponentModel.DataAnnotations;

namespace BioEngine.Core.Entities
{
    public class Site : BaseEntity
    {
        [Required] public string Title { get; set; }
        [Required] public string Url { get; set; }
    }
}
