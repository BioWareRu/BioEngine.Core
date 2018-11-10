using System;

namespace BioEngine.Core.Entities.Blocks
{
    public class TextBlock : PostBlock<TextBlockData>
    {
        public override string TypeTitle { get; set; } = "Пост";

        private (string shortText, string longText)? _texts;

        public string GetShortText()
        {
            ParseText();
            return _texts?.shortText;
        }

        private void ParseText()
        {
            if (_texts != null) return;
            if (!string.IsNullOrEmpty(Data?.Text) && Data.Text.Contains("[cut]"))
            {
                var parts = Data.Text.Split(new[] {"[cut]"}, StringSplitOptions.RemoveEmptyEntries);
                _texts = (parts[0], parts[1]);
            }
            else
            {
                _texts = (Data?.Text, null);
            }
        }

        public string GetFullText()
        {
            ParseText();
            return _texts?.longText;
        }
    }

    public class TextBlockData : ContentBlockData
    {
        public string Text { get; set; }
    }
}