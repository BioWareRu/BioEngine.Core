using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Api.Response;
using BioEngine.Core.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("v1/[controller]")]
    public class PropertiesController : Controller
    {
        private readonly IEnumerable<IPropertiesOptionsResolver> _resolvers;

        public PropertiesController(IEnumerable<IPropertiesOptionsResolver> resolvers = default)
        {
            _resolvers = resolvers;
        }

        [HttpGet]
        public async Task<ActionResult<ListResponse<PropertiesOption>>> GetAsync(
            string setKey, string propertyKey)
        {
            var propertiesSet = PropertiesProvider.GetInstance(setKey.Replace("-", "."));
            if (propertiesSet == null)
            {
                return NotFound();
            }

            var resolver = _resolvers.FirstOrDefault(r => r.CanResolve(propertiesSet));
            if (resolver == null)
            {
                return NotFound();
            }

            var options = await resolver.ResolveAsync(propertiesSet, propertyKey.Replace("-", "."));

            return new ListResponse<PropertiesOption>(options, options.Count);
        }
    }
}