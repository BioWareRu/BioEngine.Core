using System;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Storage.S3
{
    public class S3StorageModule : StorageModule<S3StorageModuleConfig>
    {
        protected override void CheckConfig()
        {
            if (Config.ServerUri == null)
            {
                throw new ArgumentException("Storage server url is empty");
            }

            if (Config.PublicUri == null)
            {
                throw new ArgumentException("Storage public url is empty");
            }

            if (string.IsNullOrEmpty(Config.Bucket))
            {
                throw new ArgumentException("S3 bucketName is empty");
            }

            if (string.IsNullOrEmpty(Config.AccessKey))
            {
                throw new ArgumentException("S3 access key is empty");
            }

            if (string.IsNullOrEmpty(Config.SecretKey))
            {
                throw new ArgumentException("S3 secret key is empty");
            }
        }

        protected override void ConfigureStorage(IServiceCollection services)
        {
            services.AddSingleton(Config);
            services.AddSingleton(new S3ClientModuleConfig(Config.ServerUri, Config.Bucket, Config.AccessKey,
                Config.SecretKey));
            services.AddSingleton<S3Client>();
            services.AddScoped<IStorage, S3Storage>();
        }
    }
}
