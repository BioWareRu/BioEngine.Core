using System;
using BioEngine.Core.Entities;
using Microsoft.AspNetCore.Routing;

namespace BioEngine.Core.Routing
{
    public static class LinkGeneratorExtensions
    {
        public static Uri GeneratePublicUrl(this LinkGenerator linkGenerator, IRoutable routable, Site site = null)
        {
            return linkGenerator.GenerateUrl(routable.PublicRouteName, new {url = routable.Url}, site);
        }

        public static Uri GenerateUrl(this LinkGenerator linkGenerator, string routeName, object routeParams,
            Site site = null)
        {
            var path = linkGenerator.GetPathByName(routeName, routeParams);
            if (site != null)
            {
                return new Uri(site.Url + path, UriKind.Absolute);
            }

            return new Uri(path, UriKind.Relative);
        }
    }
}
