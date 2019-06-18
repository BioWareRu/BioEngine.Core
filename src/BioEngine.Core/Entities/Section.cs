using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Entities
{
    [Table("Sections")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public abstract class Section : BaseSiteEntity, IContentEntity
    {
        [Required] public virtual string Type { get; set; }
        public virtual Guid? ParentId { get; set; }

        public List<ContentBlock> Blocks { get; set; }

        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        [Required] public string Url { get; set; }
        
        [NotMapped] public abstract string PublicRouteName { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    public abstract class Section<T> : Section, ITypedEntity<T> where T : ITypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped] public abstract string TypeTitle { get; set; }
    }
}
