using System.Collections.Generic;

namespace BioEngine.Core.Entities
{
    public interface IContentEntity : IEntity
    {
        string Title { get; set; }
        string Url { get; set; }
        List<ContentBlock> Blocks { get; set; }
        string PublicUrl { get; }
    }
}