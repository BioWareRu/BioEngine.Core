using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Entities
{
    [Table("Content")]
    public abstract class ContentItem : BaseSiteEntity<int>, ISectionEntity<int>, IContentEntity<int>,
        IRoutable
    {
        public virtual string Type { get; set; }
        [Required] public virtual int AuthorId { get; set; }
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        public bool IsPinned { get; set; } = false;
        public virtual int[] SectionIds { get; set; } = new int[0];
        public virtual int[] TagIds { get; set; } = new int[0];

        [NotMapped] public string PublicUrl => $"/content/{Id}-{Url}.html";
        [NotMapped] public IUser Author { get; set; }
    }

    public abstract class ContentItem<T> : ContentItem, ITypedEntity<T> where T : TypedData, new()
    {
        public virtual T Data { get; set; } = new T();
        [NotMapped] public abstract string TypeTitle { get; set; }
    }
}