using BioEngine.Core.Api.Auth;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Authorization;

namespace BioEngine.Core.Api.Controllers
{
    [Authorize(Policy = BioPolicies.Admin)]
    public class MenuController : ResponseRequestRestController<Menu, MenuRepository, Entities.Menu>
    {
        public MenuController(BaseControllerContext<Menu, MenuRepository> context) : base(context)
        {
        }
    }
}
