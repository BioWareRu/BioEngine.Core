using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioEngine.Core.Users
{
    public interface IUserDataProvider
    {
        Task<List<UserData>> GetDataAsync(int[] userIds);
    }
}