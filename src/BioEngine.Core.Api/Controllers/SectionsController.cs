using BioEngine.Core.Api.Auth;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Authorization;

namespace BioEngine.Core.Api.Controllers
{
    [Authorize(Policy = BioPolicies.Sections)]
    public class SectionsController : SectionController<Section, SectionsRepository, Entities.Section>
    {
        public SectionsController(BaseControllerContext<Section, SectionsRepository> context) : base(context)
        {
        }
    }
}
