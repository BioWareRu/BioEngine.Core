using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    public class PropertiesRecord : BaseEntity
    {
        [Required] public string Key { get; set; }
        public string EntityType { get; set; }
        public Guid EntityId { get; set; }
        public Guid? SiteId { get; set; }

        [Column(TypeName = "jsonb")]
        [Required]
        public string Data { get; set; }
    }
}
