using BioEngine.Core.DB;

namespace BioEngine.Core.Entities.Blocks
{
    [Entity("pictureblock")]
    public class PictureBlock : ContentBlock<PictureBlockData>
    {
        public override string TypeTitle { get; set; } = "Галерея";

        public override string ToString()
        {
            return $"Картинка: {Data.Picture.FileName}";
        }
    }

    public class PictureBlockData : ContentBlockData
    {
        public StorageItem Picture { get; set; } = new StorageItem();
        public string Url { get; set; } = "";
    }
}
