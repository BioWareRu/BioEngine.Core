using BioEngine.Core.DB;
using BioEngine.Core.Social;

namespace BioEngine.Extra.Twitter
{
    [Entity("twitterpublishrecord")]
    public class TwitterPublishRecord : BasePublishRecord
    {
        public long TweetId { get; set; }
    }
}
