using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    public class PropertiesRecord : BaseEntity
    {
        [NotMapped] public override string Title { get; set; } = "";
        [NotMapped] public override string Url { get; set; } = "";
        [Required] public string Key { get; set; } = "";
        public string EntityType { get; set; } = "";
        public Guid EntityId { get; set; }
        public Guid? SiteId { get; set; }

        [Column(TypeName = "jsonb")]
        [Required]
        public string Data { get; set; } = "";
    }
}
