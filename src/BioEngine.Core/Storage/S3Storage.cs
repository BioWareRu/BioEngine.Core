using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using JetBrains.Annotations;
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

        public S3Storage(IOptions<S3StorageOptions> options, ILogger<S3Storage> logger) : base(options, logger)
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

        private async Task<bool> CreateBucket(string bucketName)
        {
            try
            {
                var bucketExists = await AmazonS3Util.DoesS3BucketExistAsync(_client, bucketName);
                if (!bucketExists)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };

                    await _client.PutBucketAsync(putBucketRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }

            return true;
        }

        protected override async Task<bool> DoSave(string path, string tmpPath)
        {
            await CreateBucket(_options.Bucket);
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

        public override async Task<bool> DeleteFile(string filePath)
        {
            try
            {
                await _client.DeleteObjectAsync(_options.Bucket, filePath);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }

    public class S3StorageOptions : StorageOptions
    {
        public Uri Server { get; set; }
        public string Bucket { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}