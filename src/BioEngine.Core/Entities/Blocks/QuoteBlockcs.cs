﻿using BioEngine.Core.DB;

namespace BioEngine.Core.Entities.Blocks
{
    [Entity("quoteblock")]
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
        public string Text { get; set; } = "";
        public string Author { get; set; } = "";
        public string Link { get; set; } = "";
        public StorageItem? Picture { get; set; } = new StorageItem();
    }
}
