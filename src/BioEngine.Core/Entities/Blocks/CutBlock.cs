namespace BioEngine.Core.Entities.Blocks
{
    public class CutBlock : ContentBlock<CutBlockData>
    {
        public override string TypeTitle { get; set; } = "Кат";

        public override string ToString()
        {
            return "";
        }
    }

    public class CutBlockData : ContentBlockData
    {
        public string ButtonText { get; set; }
    }
}
