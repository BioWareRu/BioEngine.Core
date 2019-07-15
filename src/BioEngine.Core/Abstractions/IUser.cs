namespace BioEngine.Core.Abstractions
{
    public interface IUser: IEntity<string>
    {
        string Name { get; set; }
        string PhotoUrl { get; set; }
        string ProfileUrl { get; set; }
    }
}
