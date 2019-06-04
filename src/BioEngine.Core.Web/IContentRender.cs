using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Web
{
    public interface IContentRender
    {
        Task<string> RenderHtmlAsync(IContentEntity contentEntity, Site site,
            ContentEntityViewMode mode = ContentEntityViewMode.List);
    }
}
