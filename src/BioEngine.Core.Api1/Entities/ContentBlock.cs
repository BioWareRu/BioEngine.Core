using System;

namespace BioEngine.Core.Api.Entities
{
    public class ContentBlock
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string TypeTitle { get; set; }
        public int Position { get; set; }
        public object Data { get; set; }

        public static ContentBlock Create(Core.Entities.ContentBlock block)
        {
            var contentBlock = new ContentBlock
            {
                Id = block.Id,
                Type = block.Type,
                TypeTitle = block.TypeTitle,
                Position = block.Position,
                Data = block.GetData()
            };

            return contentBlock;
        }
    }
}