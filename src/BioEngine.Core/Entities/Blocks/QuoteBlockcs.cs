namespace BioEngine.Core.Entities.Blocks
{
    public class QuoteBlock : ContentBlock<QuoteBlockData>
    {
        public override string TypeTitle { get; set; } = "Цитата";

        public override string ToString()
        {
            return $"{Data.Author}: {Data.Text} ({Data.Link})";
        }
    }

    public class QuoteBlockData : ContentBlockData
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public string Link { get; set; }
    }
}
