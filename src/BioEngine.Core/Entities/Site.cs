using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Settings;

namespace BioEngine.Core.Entities
{
    public class Site : IEntity<int>
    {
        public object GetId() => Id;

        [Key] public int Id { get; set; }
        [Required] public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Url { get; set; }
        
        [NotMapped]
        public List<SettingsEntry> Settings { get; set; } = new List<SettingsEntry>();
    }
}