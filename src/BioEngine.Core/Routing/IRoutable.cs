using BioEngine.Core.Abstractions;

namespace BioEngine.Core.Routing
{
    public interface IRoutable : IEntity
    {
        string PublicRouteName { get; set; }
    }
}
