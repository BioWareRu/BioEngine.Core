using System.ComponentModel.DataAnnotations;
using BioEngine.Core.DB;
using BioEngine.Core.Social;

namespace BioEngine.Extra.Facebook.Entities
{
    [Entity("facebookpublishrecord")]
    public class FacebookPublishRecord : BasePublishRecord
    {
        [Required] public string PostId { get; set; }
    }
}
