using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Entities;

namespace BioEngine.Extra.Ads.Entities
{
    [Table("Ads")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class Ad : BaseSiteEntity
    {
        [Column(TypeName = "jsonb")]
        [Required]
        public StorageItem Picture { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
