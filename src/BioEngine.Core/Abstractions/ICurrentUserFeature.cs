namespace BioEngine.Core.Abstractions
{
    public interface ICurrentUserFeature
    {
        IUser User { get; }
        string Token { get; }
    }
}
