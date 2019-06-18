using System.ComponentModel.DataAnnotations;

namespace BioEngine.Core.Entities
{
    public class Site : BaseEntity
    {
        [Required]
        public string Url { get; set; }
    }
}
