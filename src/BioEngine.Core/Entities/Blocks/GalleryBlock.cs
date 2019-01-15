namespace BioEngine.Core.Entities.Blocks
{
    public class GalleryBlock : PostBlock<GalleryBlockData>
    {
        public override string TypeTitle { get; set; } = "Галерея";
    }

    public class GalleryBlockData : ContentBlockData
    {
        public int[] PictureIds { get; set; }
    }
}