using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;

namespace BioEngine.Core.Entities
{
    public class SettingsRecord : IEntity<int>
    {
        public object GetId() => Id;

        [Key] public int Id { get; set; }
        [Required] public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public bool IsPublished { get; set; } = true;
        public DateTimeOffset? DatePublished { get; set; } = DateTimeOffset.UtcNow;
        [Required] public string Key { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }

        [Column(TypeName = "jsonb")]
        [Required]
        public string Data { get; set; }
        
        [NotMapped]
        public Dictionary<string, SettingsBase> Settings { get; set; }
    }
}