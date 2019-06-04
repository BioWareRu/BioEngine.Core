using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Routing;

namespace BioEngine.Core.Pages.Entities
{
    [TypedEntity("page")]
    public class Page : ContentItem<PageData>
    {
        public override string TypeTitle { get; } = "Страница";
        [NotMapped] public override string PublicRouteName { get; set; } = BioEngineCoreRoutes.PagePublic;
    }

    public class PageData : ITypedData
    {
    }
}
