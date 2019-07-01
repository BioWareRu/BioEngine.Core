using System.Threading.Tasks;

namespace BioEngine.Core.Abstractions
{
    public interface ICurrentUserProvider
    {
        IUser CurrentUser { get; }
        Task<string> GetAccessTokenAsync();
    }
}
