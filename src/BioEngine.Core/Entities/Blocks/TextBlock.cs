namespace BioEngine.Core.Entities.Blocks
{
    public class TextBlock : ContentBlock<TextBlockData>
    {
        public override string TypeTitle { get; set; } = "Пост";
    }

    public class TextBlockData : ContentBlockData
    {
        public string Text { get; set; }
    }
}