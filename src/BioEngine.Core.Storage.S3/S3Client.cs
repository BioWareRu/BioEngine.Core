using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BioEngine.Core.Storage.S3
{
    public class S3Client
    {
        private readonly AmazonS3Client _client;
        private readonly ILogger<S3Client> _logger;
        private readonly S3ClientModuleConfig _options;
        private readonly TransferUtility _fileTransferUtility;

        public S3Client(ILogger<S3Client> logger, S3ClientModuleConfig options)
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
            _fileTransferUtility = new TransferUtility(_client);
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

        public async Task<bool> UploadAsync(string path, string tmpPath)
        {
            await CreateBucketAsync(_options.Bucket);
            try
            {
                using (var fileToUpload =
                    new FileStream(tmpPath, FileMode.Open))
                {
                    await _fileTransferUtility.UploadAsync(fileToUpload,
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

        public async Task<bool> DeleteAsync(string path)
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

        public async Task<bool> UploadJsonAsync<T>(T data, string objectKey)
        {
            var path = GetTmpFile();
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(data));
            await _fileTransferUtility.UploadAsync(path, _options.Bucket, objectKey);
            File.Delete(path);
            return true;
        }

        public async Task<T> DownloadJsonAsync<T>(string objectKey)
        {
            var path = GetTmpFile();
            await _fileTransferUtility.DownloadAsync(path, _options.Bucket, objectKey);
            var json = await File.ReadAllTextAsync(path);
            File.Delete(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static string GetTmpFile()
        {
            return Path.GetTempFileName();
        }
    }
}
