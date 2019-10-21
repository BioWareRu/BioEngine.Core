using System.Threading.Tasks;

namespace BioEngine.Core.Users
{
    public interface ICurrentUserProvider<TPk>
    {
        IUser<TPk> CurrentUser { get; }
        Task<string> GetAccessTokenAsync();
    }
}
