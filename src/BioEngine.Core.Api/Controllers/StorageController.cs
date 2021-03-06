using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Storage;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Core.Api.Controllers
{
    public class StorageController : ApiController
    {
        public StorageController(BaseControllerContext context) : base(context)
        {
        }

        [HttpGet]
        public Task<IEnumerable<StorageNode>> ListAsync(string path = "/")
        {
            return Storage.ListItemsAsync(path, "storage");
        }

        [HttpPost("upload")]
        public async Task<StorageNode> UploadAsync([FromQuery] string name, [FromQuery] string path = "/")
        {
            var item = await Storage.SaveFileAsync(Request.Body, name, path, "storage");
            return new StorageNode(item);
        }
        
        [HttpDelete]
        public async Task<bool> DeleteAsync([FromQuery] string path)
        {
            var result = await Storage.DeleteAsync(path, "storage");
            return result;
        }
    }
}
