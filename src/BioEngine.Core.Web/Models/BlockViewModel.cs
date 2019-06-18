using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Web.Models
{
    public struct BlockViewModel<T, TData> where T : ContentBlock<TData> where TData : ContentBlockData, new()
    {
        public BlockViewModel(T block, IContentEntity contentEntity, Site site)
        {
            Block = block;
            ContentEntity = contentEntity;
            Site = site;
        }

        public T Block { get; set; }
        public IContentEntity ContentEntity { get; set; }
        public Site Site { get; set; }
    }
}
