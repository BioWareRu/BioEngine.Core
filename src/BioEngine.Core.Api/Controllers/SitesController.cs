using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Authorization;

namespace BioEngine.Core.Api.Controllers
{
    [Authorize(Policy = BioPolicies.Admin)]
    public class SitesController : ResponseRequestRestController<Site, SitesRepository, Entities.Site>
    {
        public SitesController(BaseControllerContext<Site, SitesRepository> context) : base(context)
        {
        }
    }
}
