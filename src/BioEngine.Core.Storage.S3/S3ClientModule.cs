using System;
using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Core.Storage.S3
{
    public class S3ClientModule : BaseBioEngineModule<S3ClientModuleConfig>
    {
        protected override void CheckConfig()
        {
            if (Config.ServerUri == null)
            {
                throw new ArgumentException("Storage server url is empty");
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

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddSingleton(Config);
            services.AddSingleton<S3Client>();
        }
    }
}
