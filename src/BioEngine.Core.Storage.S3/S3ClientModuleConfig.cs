using System;

namespace BioEngine.Core.Storage.S3
{
    public class S3ClientModuleConfig
    {
        public Uri ServerUri { get; }
        public string Bucket { get; }
        public string AccessKey { get; }
        public string SecretKey { get; }

        public S3ClientModuleConfig(Uri serverUri, string bucket, string accessKey, string secretKey)
        {
            ServerUri = serverUri;
            Bucket = bucket;
            AccessKey = accessKey;
            SecretKey = secretKey;
        }
    }
}
