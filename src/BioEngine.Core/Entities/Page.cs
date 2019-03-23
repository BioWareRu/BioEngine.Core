using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Entities
{
    public class Page : BaseSiteEntity, IContentEntity
    {
        [Required] public virtual string Title { get; set; }
        [Required] public virtual string Url { get; set; }
        
        [InverseProperty(nameof(ContentBlock.Page))]
        public List<ContentBlock> Blocks { get; set; }
    }
}
