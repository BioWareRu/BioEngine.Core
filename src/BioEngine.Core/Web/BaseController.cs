using BioEngine.Core.Properties;
using BioEngine.Core.Storage;
using BioEngine.Core.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Web
{
    public abstract class BaseController : Controller
    {
        protected ILogger Logger { get; }
        protected IStorage Storage { get; }
        protected PropertiesProvider PropertiesProvider { get; }

        protected BaseController(BaseControllerContext context)
        {
            Logger = context.Logger;
            Storage = context.Storage;
            PropertiesProvider = context.PropertiesProvider;
        }

        protected IUser CurrentUser
        {
            get
            {
                var feature = HttpContext.Features.Get<ICurrentUserFeature>();
                return feature.User;
            }
        }
    }
}