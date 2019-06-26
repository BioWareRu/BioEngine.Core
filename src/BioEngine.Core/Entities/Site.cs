using System.ComponentModel.DataAnnotations;
using BioEngine.Core.DB;

namespace BioEngine.Core.Entities
{
    [Entity("site")]
    public class Site : BaseEntity
    {
        [Required]
        public string Url { get; set; }
    }
}
