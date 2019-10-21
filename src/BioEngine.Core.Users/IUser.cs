using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Users
{
    public interface IUser<TPk>: IEntity<TPk>
    {
        string Name { get; set; }
        string PhotoUrl { get; set; }
        string ProfileUrl { get; set; }
    }
}
