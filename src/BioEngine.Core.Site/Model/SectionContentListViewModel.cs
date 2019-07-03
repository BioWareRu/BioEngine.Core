using BioEngine.Core.Entities;

namespace BioEngine.Core.Site.Model
{
    public class SectionContentListViewModel<TSection, TContent> : ListViewModel<TContent>
        where TContent : ContentItem where TSection : Section
    {
        public new TSection Section { get; }

        public SectionContentListViewModel(PageViewModelContext context, TSection section, TContent[] items,
            int totalItems, int page,
            int itemsPerPage) : base(context, items, totalItems, page, itemsPerPage)
        {
            Section = section;
        }
    }
}