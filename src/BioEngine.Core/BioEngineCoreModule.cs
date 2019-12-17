using BioEngine.Core.DB;
using BioEngine.Core.Entities.Blocks;
using BioEngine.Core.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core
{
    public class BioEngineCoreModule : BaseBioEngineModule<BioEngineCoreModuleConfig>
    {
    }

    public class BioEngineCoreModuleConfig
    {
        public BioEngineCoreModuleConfig(string version)
        {
            Version = version;
        }

        public string Version { get; }
    }

    public class CoreBioContextModelConfigurator : IBioContextModelConfigurator
    {
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterContentBlock<CutBlock, CutBlockData>(logger);
            modelBuilder.RegisterContentBlock<FileBlock, FileBlockData>(logger);
            modelBuilder.RegisterContentBlock<GalleryBlock, GalleryBlockData>(logger);
            modelBuilder.RegisterContentBlock<IframeBlock, IframeBlockData>(logger);
            modelBuilder.RegisterContentBlock<PictureBlock, PictureBlockData>(logger);
            modelBuilder.RegisterContentBlock<QuoteBlock, QuoteBlockData>(logger);
            modelBuilder.RegisterContentBlock<TextBlock, TextBlockData>(logger);
            modelBuilder.RegisterContentBlock<TwitterBlock, TwitterBlockData>(logger);
            modelBuilder.RegisterContentBlock<YoutubeBlock, YoutubeBlockData>(logger);
        }
    }
}
