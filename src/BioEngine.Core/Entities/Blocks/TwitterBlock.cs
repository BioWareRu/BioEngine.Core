namespace BioEngine.Core.Entities.Blocks
{
    public class TwitterBlock : ContentBlock<TwitterBlockData>
    {
        public override string TypeTitle { get; set; } = "Twitter";
        
        public override string ToString()
        {
            return $"Twitter: {Data.TwitterId.ToString()}";
        }
    }

    public class TwitterBlockData : ContentBlockData
    {
        public long TwitterId { get; set; }
    }
}
