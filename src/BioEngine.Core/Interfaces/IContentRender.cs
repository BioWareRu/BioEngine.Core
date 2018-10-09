using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Interfaces
{
    public interface IContentRender
    {
        Task<string> RenderHtml(ContentItem contentItem);
    }
}