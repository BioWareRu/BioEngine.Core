using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;

namespace BioEngine.Core.Entities
{
    public class Page : IEntity<int>, ISiteEntity<int>
    {
        public object GetId() => Id;

        [Key] public int Id { get; set; }
        [Required] public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        public int[] SiteIds { get; set; } = new int[0];
        

        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        [Required] public string Text { get; set; }
        
        [NotMapped]
        public Dictionary<string, SettingsBase> Settings { get; set; } = new Dictionary<string, SettingsBase>();
    }
}