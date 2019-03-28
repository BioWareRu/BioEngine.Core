using System.Threading.Tasks;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Web
{
    public interface IContentRender
    {
        Task<string> RenderHtmlAsync(Post post);
    }
}
