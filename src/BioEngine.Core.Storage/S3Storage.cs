using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Core.Storage
{
    [UsedImplicitly]
    public class S3Storage : Storage
    {
        private readonly ILogger<S3Storage> _logger;
        private readonly S3StorageOptions _options;
        private readonly AmazonS3Client _client;

        public S3Storage(IOptions<S3StorageOptions> options, StorageItemsRepository repository,
            BioContext dbContext,
            ILogger<S3Storage> logger) : base(options, repository, dbContext, logger)
        {
            _logger = logger;
            _options = options.Value;

            var config = new AmazonS3Config
            {
                RegionEndpoint =
                    RegionEndpoint
                        .USEast1, // MUST set this before setting ServiceURL and it should match the `MINIO_REGION` enviroment variable.
                ServiceURL = _options.Server.ToString(), // replace http://localhost:9000 with URL of your minio server
                ForcePathStyle = true // MUST be true to work correctly with Minio server
            };
            _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, config);
        }

        private async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                var bucketExists = await AmazonS3Util.DoesS3BucketExistAsync(_client, bucketName);
                if (!bucketExists)
                {
                    var putBucketRequest = new PutBucketRequest {BucketName = bucketName, UseClientRegion = true};

                    await _client.PutBucketAsync(putBucketRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        protected override async Task<bool> DoSaveAsync(string path, string tmpPath)
        {
            await CreateBucketAsync(_options.Bucket);
            var fileTransferUtility =
                new TransferUtility(_client);
            try
            {
                using (var fileToUpload =
                    new FileStream(tmpPath, FileMode.Open))
                {
                    await fileTransferUtility.UploadAsync(fileToUpload,
                        _options.Bucket, path);
                    File.Delete(tmpPath);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        protected override async Task<bool> DoDeleteAsync(string path)
        {
            try
            {
                var result = await _client.DeleteObjectAsync(_options.Bucket, path);
                return result.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return false;
            }
        }
    }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class S3StorageOptions : StorageOptions
    {
        public Uri Server { get; set; }
        public string Bucket { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

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
            services.Configure<S3StorageOptions>(o =>
            {
                o.PublicUri = Config.PublicUri!;
                o.Server = Config.ServerUri!;
                o.Bucket = Config.Bucket;
                o.AccessKey = Config.AccessKey;
                o.SecretKey = Config.SecretKey;
            });
            services.AddScoped<IStorage, S3Storage>();
        }
    }

    public class S3StorageModuleConfig : StorageModuleConfig
    {
        public Uri? ServerUri { get; set; }
        public Uri? PublicUri { get; set; }
        public string Bucket { get; set; } = "";
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
    }
}
