using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.DB;

namespace BioEngine.Core.Entities
{
    [Entity("propertiesrecord")]
    public class PropertiesRecord : BaseEntity
    {
        [NotMapped] public override string Title { get; set; } = "";
        [Required] public string Key { get; set; } = "";
        public string EntityType { get; set; } = "";
        public Guid EntityId { get; set; }
        public Guid? SiteId { get; set; }

        [Column(TypeName = "jsonb")]
        [Required]
        public string Data { get; set; } = "";
    }
}
