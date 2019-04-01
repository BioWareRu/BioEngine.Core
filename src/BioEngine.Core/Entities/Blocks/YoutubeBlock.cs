namespace BioEngine.Core.Entities.Blocks
{
    public class YoutubeBlock : ContentBlock<YoutubeBlockData>
    {
        public override string TypeTitle { get; set; } = "Youtube";

        public override string ToString()
        {
            return $"Youtube: {Data.YoutubeId}";
        }
    }

    public class YoutubeBlockData : ContentBlockData
    {
        public string YoutubeId { get; set; }
    }
}
