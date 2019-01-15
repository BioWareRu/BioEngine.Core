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
        [Required] public virtual int LogoId { get; set; }
        [Required] public virtual int LogoSmallId { get; set; }
        [Required] public virtual string ShortDescription { get; set; }
        [Required] public virtual string Hashtag { get; set; }

        [NotMapped] public string PublicUrl => $"/section/{Id}-{Url}.html";

        [ForeignKey(nameof(LogoId))] public StorageItem Logo { get; set; }
        [ForeignKey(nameof(LogoSmallId))] public StorageItem LogoSmall { get; set; }
    }

    public abstract class Section<T> : Section, ITypedEntity<T> where T : TypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped] public abstract string TypeTitle { get; set; }
    }
}