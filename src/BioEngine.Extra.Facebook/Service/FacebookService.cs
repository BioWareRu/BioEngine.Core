using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Flurl.Http;

namespace BioEngine.Extra.Facebook.Service
{
    public class FacebookService
    {
        private readonly ILogger<FacebookService> _logger;

        public FacebookService(ILogger<FacebookService> logger)
        {
            _logger = logger;
        }

        [SuppressMessage("ReSharper", "RCS1198")]
        public async Task<string?> PostLinkAsync(Uri link, FacebookConfig config)
        {
            _logger.LogDebug("Post new link to facebook");
            try
            {
                var response =
                    await $"{config.Url}/{config.PageId}/feed"
                        .SetQueryParam("link", link.ToString())
                        .SetQueryParam("access_token", config.AccessToken)
                        .WithTimeout(60)
                        .PostUrlEncodedAsync(new { });
                var data = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error while sending facebook request");
                    throw new Exception($"Bad facebook response: {data}");
                }

                var postResponse = JsonConvert.DeserializeObject<FacebookNewPostResponse>(data);
                _logger.LogDebug("Link posted to facebook");
                return postResponse.Id;
            }
            catch (FlurlHttpException ex)
            {
                _logger.LogError(ex, "Error in request to Facebook: {errorText}. Response: {response}", ex.ToString(),
                    await ex.GetResponseStringAsync());
                return null;
            }
        }

        [SuppressMessage("ReSharper", "RCS1198")]
        public async Task<bool> DeletePostAsync(string postId, FacebookConfig config)
        {
            _logger.LogDebug("Delete post from facebook");
            var response =
                await $"{config.Url}/{postId}"
                    .SetQueryParam("access_token", config.AccessToken)
                    .DeleteAsync();
            var data = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error while sending facebook request");
                throw new Exception($"Bad facebook response: {data}");
            }

            var postResponse = JsonConvert.DeserializeObject<FacebookDeleteResponse>(data);
            _logger.LogDebug("Post deleted from facebook: {response}", (postResponse.Success ? "Yes" : "No"));
            return postResponse.Success;
        }

        public class FacebookNewPostResponse
        {
            [JsonProperty("id")] public string? Id { get; set; }
        }

        public class FacebookDeleteResponse
        {
            [JsonProperty("success")] public bool Success { get; set; }
        }
    }
}
