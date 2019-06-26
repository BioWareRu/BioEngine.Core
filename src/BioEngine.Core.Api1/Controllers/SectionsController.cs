using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;

namespace BioEngine.Core.Api.Controllers
{
    public class SectionsController : SectionController<Section, SectionsRepository, Entities.Section>
    {
        public SectionsController(BaseControllerContext<Section, SectionsRepository> context) : base(context)
        {
        }
    }
}
