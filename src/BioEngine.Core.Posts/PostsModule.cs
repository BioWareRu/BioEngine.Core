using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using BioEngine.Core.Posts.Search;
using BioEngine.Core.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BioEngine.Core.Posts
{
    public abstract class PostsModule<TUserPk> : BaseBioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.RegisterSearchProvider<PostsSearchProvider<TUserPk>, Post<TUserPk>>();
        }

        public override void ConfigureDbContext(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureDbContext(services, configuration, environment);
            services.AddScoped<IBioRepository<Post<TUserPk>>, PostsRepository<TUserPk>>();
            services.AddScoped<PostsRepository<TUserPk>, PostsRepository<TUserPk>>();
            services.AddScoped<IRepositoryHook, UpdateSiteIdsHook<TUserPk>>();
            services.AddSingleton<IBioContextModelConfigurator, PostsDbContextConfigurator<TUserPk>>();
        }
    }

    public class PostsDbContextConfigurator<TUserPk> : IBioContextModelConfigurator
    {
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterContentItem<Post<TUserPk>>(logger);
            modelBuilder.RegisterEntity<PostVersion<TUserPk>>();
            modelBuilder.RegisterEntity<PostTemplate<TUserPk>>();
            modelBuilder.Entity<PostTemplate<TUserPk>>().Property(c => c.Data).HasConversion(
                d => JsonConvert.SerializeObject(d,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto
                    }),
                j => JsonConvert.DeserializeObject<PostTemplateData>(j,
                    new JsonSerializerSettings
                    {
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                        TypeNameHandling = TypeNameHandling.Auto
                    }));
        }
    }
}
