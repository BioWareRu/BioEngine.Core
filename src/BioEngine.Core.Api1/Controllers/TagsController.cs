using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Web;

namespace BioEngine.Core.Api.Controllers
{
    public class TagsController : ResponseRequestRestController<Tag, TagsRepository, Entities.Tag>
    {
        public TagsController(BaseControllerContext<Tag, TagsRepository> context) : base(context)
        {
        }
    }
}
