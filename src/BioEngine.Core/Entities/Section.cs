using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    [Table("Sections")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public abstract class Section : BaseSiteEntity, IContentEntity
    {
        [Required] public virtual string Type { get; set; }
        public virtual Guid? ParentId { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public virtual StorageItem Logo { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public virtual StorageItem LogoSmall { get; set; }

        [InverseProperty(nameof(ContentBlock.Section))]
        public List<ContentBlock> Blocks { get; set; }

        [Required] public virtual string Hashtag { get; set; }


        public abstract string PublicUrl { get; }
        public abstract string PostsUrl { get; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    public abstract class Section<T> : Section, ITypedEntity<T> where T : ITypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped] public abstract string TypeTitle { get; set; }
    }
}
