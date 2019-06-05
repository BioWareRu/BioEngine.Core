namespace BioEngine.Core.Abstractions
{
    public interface IRoutable : IEntity
    {
        string PublicRouteName { get; set; }
    }
}
