using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Site.Model
{
    public class ListViewModel<TEntity> : PageViewModel where TEntity : class, IEntity
    {
        public TEntity[] Items { get; }
        public int TotalItems { get; }

        public int Page { get; }
        public int ItemsPerPage { get; }

        public Tag[] Tags { get; set; }

        public ListViewModel(PageViewModelContext context, TEntity[] items, int totalItems, int page,
            int itemsPerPage) :
            base(context)
        {
            Items = items;
            TotalItems = totalItems;
            Page = page;
            ItemsPerPage = itemsPerPage;
        }

        public override async Task<PageMetaModel> GetMetaAsync()
        {
            var meta = await base.GetMetaAsync();
            if (Tags != null && Tags.Any())
            {
                meta.Title = $"{string.Join(", ", Tags.Select(t => t.Title))} / {Site.Title}";
            }

            return meta;
        }

        public PageViewModelContext GetContext()
        {
            return new PageViewModelContext(LinkGenerator, PropertiesProvider, Site, Section);
        }
    }
}