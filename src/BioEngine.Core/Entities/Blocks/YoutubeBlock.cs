using System;

namespace BioEngine.Core.Entities.Blocks
{
    public class YoutubeBlock : PostBlock<YoutubeBlockData>
    {
        public override string TypeTitle { get; set; } = "Youtube";
    }

    public class YoutubeBlockData : ContentBlockData
    {
        public Uri Url { get; set; }
    }
}