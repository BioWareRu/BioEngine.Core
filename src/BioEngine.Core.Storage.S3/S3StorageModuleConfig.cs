using System;

namespace BioEngine.Core.Storage.S3
{
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