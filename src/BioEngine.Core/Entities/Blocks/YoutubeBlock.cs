namespace BioEngine.Core.Entities.Blocks
{
    public class YoutubeBlock : ContentBlock<YoutubeBlockData>
    {
        public override string TypeTitle { get; set; } = "Youtube";
    }

    public class YoutubeBlockData : ContentBlockData
    {
        public string YoutubeId { get; set; }
    }
}
