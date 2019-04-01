using System.Linq;

namespace BioEngine.Core.Entities.Blocks
{
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
        public StorageItem[] Pictures { get; set; }
    }
}
