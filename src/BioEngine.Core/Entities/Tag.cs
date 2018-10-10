using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;

namespace BioEngine.Core.Entities
{
    [Table("Tags")]
    public class Tag : IEntity<int>
    {
        public object GetId() => Id;

        [Key] public virtual int Id { get; set; }
        public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public bool IsPublished { get; set; } = true;
        public DateTimeOffset? DatePublished { get; set; } = DateTimeOffset.UtcNow;
        public string Name { get; set; }
        
        [NotMapped]
        public List<SettingsEntry> Settings { get; set; } = new List<SettingsEntry>();
    }
}