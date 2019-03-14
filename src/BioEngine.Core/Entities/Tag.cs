using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    [Table("Tags")]
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
    }
}
