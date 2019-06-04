using BioEngine.Core.Entities;

namespace BioEngine.Core.Web.Models
{
    public struct BlockViewModel<T, TData> where T : ContentBlock<TData> where TData : ContentBlockData, new()
    {
        public BlockViewModel(T block, ContentItem contentEntity, Site site)
        {
            Block = block;
            ContentEntity = contentEntity;
            Site = site;
        }

        public T Block { get; set; }
        public ContentItem ContentEntity { get; set; }
        public Site Site { get; set; }
    }
}
