namespace BioEngine.Core.Entities.Blocks
{
    public class IframeBlock : ContentBlock<IframeBlockData>
    {
        public override string TypeTitle { get; set; } = "Iframe";
        
        public override string ToString()
        {
            return $"Frame: {Data.Src}";
        }
    }

    public class IframeBlockData : ContentBlockData
    {
        public string Src { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
