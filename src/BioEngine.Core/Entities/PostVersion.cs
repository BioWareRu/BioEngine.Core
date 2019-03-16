using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    public class PostVersion : BaseEntity
    {
        public Guid PostId { get; set; }
        [Column(TypeName = "jsonb")] public string Data { get; set; }
    }
}
