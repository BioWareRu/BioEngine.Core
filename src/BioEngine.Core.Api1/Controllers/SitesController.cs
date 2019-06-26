using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;

namespace BioEngine.Core.Api.Controllers
{
    public class
        SitesController : ResponseRequestRestController<Site, SitesRepository, Entities.Site>
    {
        public SitesController(BaseControllerContext<Site, SitesRepository> context) : base(context)
        {
        }
    }
}
