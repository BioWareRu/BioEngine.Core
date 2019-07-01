using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Users
{
    public class TestUserDataProvider : IUserDataProvider
    {
        public Task<List<IUser>> GetDataAsync(string[] userIds)
        {
            var users = userIds
                .Select(userId =>
                    new TestUser {Id = userId, Name = $"User{userId.ToString()}", PhotoUrl = "", ProfileUrl = ""})
                .Cast<IUser>().ToList();

            return Task.FromResult(users);
        }
    }

    public class TestUser : IUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string ProfileUrl { get; set; }
    }
}
