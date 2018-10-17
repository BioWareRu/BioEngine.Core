using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Interfaces
{
    public interface IContentRender
    {
        Task<string> RenderHtmlAsync(ContentItem contentItem);
    }
}