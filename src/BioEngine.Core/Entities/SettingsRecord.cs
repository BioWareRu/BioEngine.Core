using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    public class SettingsRecord : BaseEntity<int>
    {
        [Required] public string Key { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public int? SiteId { get; set; }

        [Column(TypeName = "jsonb")]
        [Required]
        public string Data { get; set; }
    }
}