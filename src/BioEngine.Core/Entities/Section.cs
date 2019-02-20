using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Storage;

namespace BioEngine.Core.Entities
{
    [Table("Sections")]
    public abstract class Section : BaseSiteEntity<int>, IRoutable
    {
        [Required] public virtual string Type { get; set; }
        public virtual int? ParentId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public virtual StorageItem Logo { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public virtual StorageItem LogoSmall { get; set; }

        [Required] public virtual string ShortDescription { get; set; }
        [Required] public virtual string Hashtag { get; set; }

        [NotMapped] public string PublicUrl => $"/section/{Id}-{Url}.html";
    }

    public abstract class Section<T> : Section, ITypedEntity<T> where T : TypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped] public abstract string TypeTitle { get; set; }
    }
}
