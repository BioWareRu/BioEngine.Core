using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioEngine.Core.Abstractions
{
    public interface IUserDataProvider
    {
        Task<List<IUser>> GetDataAsync(string[] userIds);
    }
}
