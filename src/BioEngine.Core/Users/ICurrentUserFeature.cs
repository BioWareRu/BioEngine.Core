namespace BioEngine.Core.Users
{
    public interface ICurrentUserFeature
    {
        IUser User { get; }
        string Token { get; }
    }
}