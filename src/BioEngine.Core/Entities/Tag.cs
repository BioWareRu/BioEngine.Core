using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    [Table("Tags")]
    public class Tag : BaseEntity
    {
        [NotMapped] public override string Url { get; set; }
    }
}
