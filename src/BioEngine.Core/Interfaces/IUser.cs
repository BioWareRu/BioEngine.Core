namespace BioEngine.Core.Interfaces
{
    public interface IUser
    {
        int Id { get; set; }
        string Name { get; set; }
        string PhotoUrl { get; set; }
        string ProfileUrl { get; set; }
    }
}