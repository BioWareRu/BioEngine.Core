namespace BioEngine.Core.Interfaces
{
    public interface ICurrentUserFeature
    {
        IUser User { get; }
        string Token { get; }
    }
}