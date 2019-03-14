namespace BioEngine.Core.Entities.Blocks
{
    public class TwitterBlock : PostBlock<TwitterBlockData>
    {
        public override string TypeTitle { get; set; } = "Twitter";
    }

    public class TwitterBlockData : ContentBlockData
    {
        public long TwitterId { get; set; }
    }
}