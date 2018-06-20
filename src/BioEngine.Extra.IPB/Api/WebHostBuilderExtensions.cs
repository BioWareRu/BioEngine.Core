﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB.Api
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder AddIPB(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.ConfigureServices(
                (context, services) =>
                {
                    bool.TryParse(context.Configuration["IPB_API_DEV_MODE"] ?? "", out var devMode);
                    int.TryParse(context.Configuration["IPB_API_ADMIN_GROUP_ID"], out var adminGroupId);
                    int.TryParse(context.Configuration["IPB_API_PUBLISHER_GROUP_ID"], out var publisherGroupId);
                    int.TryParse(context.Configuration["IPB_API_EDITOR_GROUP_ID"], out var editorGroupId);
                    services.Configure<IPBApiConfig>(config =>
                    {
                        config.ApiUrl = new Uri(context.Configuration["IPB_API_URL"]);
                        config.DevMode = devMode;
                        config.AdminGroupId = adminGroupId;
                        config.PublisherGroupId = publisherGroupId;
                        config.EditorGroupId = editorGroupId;
                        config.ClientId = context.Configuration["IPB_API_CLIENT_ID"];
                    });
                    services.AddSingleton<IPBApiClientFactory>();
                });
            return webHostBuilder;
        }
    }
}