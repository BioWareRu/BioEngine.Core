namespace BioEngine.Core.Entities.Blocks
{
    public class CutBlock : PostBlock<CutBlockData>
    {
        public override string TypeTitle { get; set; } = "Кат";
    }

    public class CutBlockData : ContentBlockData
    {
        public string ButtonText { get; set; }
    }
}