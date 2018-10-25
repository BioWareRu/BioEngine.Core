using System;

namespace BioEngine.Core.Users
{
    public class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Uri ProfileLink { get; set; }
        public Uri AvatarLink { get; set; }
    }
}