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

namespace BioEngine.Core.Storage
{
    [UsedImplicitly]
    public class S3Storage : Storage
    {
        private readonly ILogger<S3Storage> _logger;
        private readonly S3StorageModuleConfig _options;
        private readonly AmazonS3Client _client;

        public S3Storage(S3StorageModuleConfig options, StorageItemsRepository repository,
            BioContext dbContext,
            ILogger<S3Storage> logger) : base(options, repository, dbContext, logger)
        {
            _logger = logger;
            _options = options;

            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1,
                ServiceURL = _options.ServerUri!.ToString(),
                ForcePathStyle = true
            };
            _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, config);
        }

        private async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_client, bucketName);
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
            services.AddScoped<IStorage, S3Storage>();
        }
    }

    public class S3StorageModuleConfig : StorageModuleConfig
    {
        public Uri ServerUri { get; }
        public string Bucket { get; }
        public string AccessKey { get; }
        public string SecretKey { get; }

        public S3StorageModuleConfig(Uri publicUri, Uri serverUri, string bucket, string accessKey, string secretKey) :
            base(publicUri)
        {
            ServerUri = serverUri;
            Bucket = bucket;
            AccessKey = accessKey;
            SecretKey = secretKey;
        }
    }
}
