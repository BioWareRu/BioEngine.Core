using System.IO;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Repository;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Storage.S3
{
    [UsedImplicitly]
    public class S3Storage : Storage
    {
        private readonly S3Client _s3Client;

        public S3Storage(S3StorageModuleConfig options, StorageItemsRepository repository,
            BioContext dbContext,
            S3Client s3Client,
            ILogger<S3Storage> logger) : base(options, repository, dbContext, logger)
        {
            _s3Client = s3Client;
        }


        protected override Task<bool> DoSaveAsync(string path, Stream file)
        {
            return _s3Client.UploadAsync(path, file);
        }

        protected override Task<bool> DoDeleteAsync(string path)
        {
            return _s3Client.DeleteAsync(path);
        }
    }
}
