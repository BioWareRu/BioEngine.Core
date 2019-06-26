using BioEngine.Core.DB;

namespace BioEngine.Core.Entities.Blocks
{
    [Entity("textblock")]
    public class TextBlock : ContentBlock<TextBlockData>
    {
        public override string TypeTitle { get; set; } = "Пост";
        
        public override string ToString()
        {
            return Data.Text;
        }
    }

    public class TextBlockData : ContentBlockData
    {
        public string Text { get; set; } = "";
    }
}
