using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Api
{
    [ApiController]
    [Authorize]
    [Route("v1/[controller]")]
    public abstract class ApiController : BaseController
    {
        protected ApiController(BaseControllerContext context) : base(context)
        {
        }

        protected async Task<byte[]> GetBodyAsFileAsync()
        {
            using (var ms = new MemoryStream())
            {
                await Request.Body.CopyToAsync(ms);
                return ms.GetBuffer();
            }
        }
    }
}