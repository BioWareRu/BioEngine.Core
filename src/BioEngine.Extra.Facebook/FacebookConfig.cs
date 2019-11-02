using System;
using BioEngine.Core.Social;

namespace BioEngine.Extra.Facebook
{
    public class FacebookConfig : IContentPublisherConfig
    {
        public FacebookConfig(Uri url, string pageId, string accessToken)
        {
            if (string.IsNullOrEmpty(pageId))
            {
                throw new ArgumentException("Facebook page id cannot be empty}");
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Facebook access token cannot be empty}");
            }

            Url = url;
            PageId = pageId;
            AccessToken = accessToken;
        }

        public Uri Url { get; }
        public string PageId { get; }
        public string AccessToken { get; }
    }
}
