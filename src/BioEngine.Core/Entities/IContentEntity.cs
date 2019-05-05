using System.Collections.Generic;

namespace BioEngine.Core.Entities
{
    public interface IContentEntity : ISiteEntity, IRoutable
    {
        List<ContentBlock> Blocks { get; set; }
    }
}
