using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Core.Posts.Db;
using BioEngine.Core.Posts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BioEngine.Core.Posts
{
    public class PostTemplatesModule<TUserPk> : BaseBioEngineModule
    {
        public override void ConfigureDbContext(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureDbContext(services, configuration, environment);
            services.AddScoped<IBioRepository<PostTemplate<TUserPk>>, PostTemplatesRepository<TUserPk>>();
            services.AddScoped<PostTemplatesRepository<TUserPk>, PostTemplatesRepository<TUserPk>>();
            services.AddSingleton<IBioContextModelConfigurator, PostTemplatesDbContextConfigurator<TUserPk>>();
        }
    }
    
    public class PostTemplatesDbContextConfigurator<TUserPk> : IBioContextModelConfigurator
    {
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
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
