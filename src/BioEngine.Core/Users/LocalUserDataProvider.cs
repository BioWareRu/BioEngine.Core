using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Users
{
    public class LocalUserDataProvider : IUserDataProvider
    {
        public Task<List<IUser>> GetDataAsync(int[] userIds)
        {
            var users = userIds
                .Select(userId =>
                    new LocalUser {Id = userId, Name = $"User{userId.ToString()}", PhotoUrl = "", ProfileUrl = ""})
                .Cast<IUser>().ToList();

            return Task.FromResult(users);
        }
    }

    public class LocalUser : IUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string ProfileUrl { get; set; }
    }
}
