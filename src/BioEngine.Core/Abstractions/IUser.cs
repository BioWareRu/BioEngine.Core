namespace BioEngine.Core.Abstractions
{
    public interface IUser
    {
        string Id { get; set; }
        string Name { get; set; }
        string PhotoUrl { get; set; }
        string ProfileUrl { get; set; }
    }
}
