using System.ComponentModel.DataAnnotations;

namespace BioEngine.Core.Entities
{
    public class Site : BaseEntity<int>
    {
        [Required] public string Title { get; set; }
        [Required] public string Url { get; set; }
    }
}