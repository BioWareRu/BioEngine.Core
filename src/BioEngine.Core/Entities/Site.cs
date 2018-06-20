using System;
using System.ComponentModel.DataAnnotations;
using BioEngine.Core.Core;

namespace BioEngine.Core.Entities
{
    public class Site : IEntity<int>
    {
        [Key] public int Id { get; set; }
        [Required] public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Url { get; set; }
        [Required] public string Description { get; set; }
        [Required] public string Keywords { get; set; }
    }
}