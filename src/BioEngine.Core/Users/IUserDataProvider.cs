using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioEngine.Core.Users
{
    public interface IUserDataProvider
    {
        Task<List<IUser>> GetDataAsync(int[] userIds);
    }
}