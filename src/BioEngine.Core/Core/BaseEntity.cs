using System;
using System.ComponentModel.DataAnnotations;

namespace BioEngine.Core.Core
{
    public abstract class BaseEntity
    {
        [Required] public virtual DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
        [Required] public virtual DateTimeOffset DateUpdated { get; set; } = DateTimeOffset.UtcNow;
    }
}