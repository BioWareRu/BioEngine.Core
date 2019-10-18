using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioEngine.Core.Users
{
    public interface IUserDataProvider<TPk>
    {
        Task<List<IUser<TPk>>> GetDataAsync(TPk[] userIds);
    }
}
