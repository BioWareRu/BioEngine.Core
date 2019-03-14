namespace BioEngine.Core.Entities.Blocks
{
    public class IframeBlock : PostBlock<IframeBlockData>
    {
        public override string TypeTitle { get; set; } = "Iframe";
    }

    public class IframeBlockData : ContentBlockData
    {
        public string Src { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
