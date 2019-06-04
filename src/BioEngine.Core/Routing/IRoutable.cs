using BioEngine.Core.Entities;

namespace BioEngine.Core.Routing
{
    public interface IRoutable : IEntity
    {
        string PublicRouteName { get; set; }
    }
}
