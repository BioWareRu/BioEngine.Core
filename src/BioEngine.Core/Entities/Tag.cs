using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.DB;

namespace BioEngine.Core.Entities
{
    [Table("Tags")]
    [Entity("tag")]
    public class Tag : BaseEntity
    {
        [Required]
        public string Title { get; set; }
    }
}
