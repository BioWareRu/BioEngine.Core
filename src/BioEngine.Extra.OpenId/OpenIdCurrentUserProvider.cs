using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BioEngine.Core.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BioEngine.Extra.OpenId
{
    public class OpenIdCurrentUserProvider<TUser, TUserPk> : ICurrentUserProvider<TUserPk>
        where TUser : IUser<TUserPk>, new()
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IUser<TUserPk> CurrentUser =>
            _httpContextAccessor.HttpContext.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier)
                ? new TUser
                {
                    Id = JsonConvert.DeserializeObject<TUserPk>(
                        $"\"{_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value}\""),
                    Name =
                        _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)
                            ?.Value,
                    PhotoUrl =
                        _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "photo")?.Value,
                    ProfileUrl = _httpContextAccessor.HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.Webpage)?.Value
                }
                : (IUser<TUserPk>) null;

        public OpenIdCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<string> GetAccessTokenAsync()
        {
            return _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        }
    }
}
