using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Interfaces;

namespace BioEngine.Core.Users
{
    public interface IUserDataProvider
    {
        Task<List<IUser>> GetDataAsync(int[] userIds);
    }
}