using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Abstractions
{
    public interface IContentItem : IContentEntity, ISectionEntity
    {
        [NotMapped] List<Section> Sections { get; set; }
    }
}
