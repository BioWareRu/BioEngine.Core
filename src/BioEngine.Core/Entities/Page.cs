using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioEngine.Core.Entities
{
    public class Page : BaseSiteEntity, IContentEntity
    {
        [InverseProperty(nameof(ContentBlock.Page))]
        public List<ContentBlock> Blocks { get; set; }

        [NotMapped] public string PublicUrl => $"/page/{Url}.html";
    }
}
