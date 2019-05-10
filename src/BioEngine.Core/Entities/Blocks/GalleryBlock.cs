using System.Linq;
using BioEngine.Core.DB;

namespace BioEngine.Core.Entities.Blocks
{
    [TypedEntity("gallery")]
    public class GalleryBlock : ContentBlock<GalleryBlockData>
    {
        public override string TypeTitle { get; set; } = "Галерея";

        public override string ToString()
        {
            return $"Галерея: {string.Join(", ", Data.Pictures.Select(p => p.FileName))}";
        }
    }

    public class GalleryBlockData : ContentBlockData
    {
        public StorageItem[] Pictures { get; set; } = new StorageItem[0];
    }
}
