namespace BioEngine.Core.Entities.Blocks
{
    public class GalleryBlock : ContentBlock<GalleryBlockData>
    {
        public override string TypeTitle { get; set; } = "Галерея";
    }

    public class GalleryBlockData : ContentBlockData
    {
        public StorageItem[] Pictures { get; set; }
    }
}
