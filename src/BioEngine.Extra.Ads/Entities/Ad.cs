using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Extra.Ads.Entities
{
    [Table("Ads")]
    [Entity("ad")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class Ad : BaseSiteEntity, IContentEntity
    {
        [NotMapped] public string PublicRouteName { get; set; } = "";
        [Required] public string Url { get; set; }
        public string Title { get; set; }
        [NotMapped] public List<ContentBlock> Blocks { get; set; } = new List<ContentBlock>();
        public bool IsPublished { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
