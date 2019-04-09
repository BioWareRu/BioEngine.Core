using BioEngine.Core.DB;

namespace BioEngine.Core.Entities.Blocks
{
    [TypedEntity("twitter")]
    public class TwitterBlock : ContentBlock<TwitterBlockData>
    {
        public override string TypeTitle { get; set; } = "Twitter";

        public override string ToString()
        {
            return $"Twitter: {Data.TweetId} by {Data.TweetAuthor}";
        }
    }

    public class TwitterBlockData : ContentBlockData
    {
        public string TweetId { get; set; }
        public string TweetAuthor { get; set; }
    }
}
