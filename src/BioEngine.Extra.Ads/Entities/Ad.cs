using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;

namespace BioEngine.Extra.Ads.Entities
{
    [Table("Ads")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class Ad : BaseSiteEntity, IContentEntity
    {
        [NotMapped] public string PublicRouteName { get; set; } = "";
        [Required] public string Url { get; set; }
        public List<ContentBlock> Blocks { get; set; }
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
