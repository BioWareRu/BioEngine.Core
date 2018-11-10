using BioEngine.Core.Storage;

namespace BioEngine.Core.Entities.Blocks
{
    public class FileBlock : PostBlock<FileBlockData>
    {
        public override string TypeTitle { get; set; } = "Файл";
    }

    public class FileBlockData : ContentBlockData
    {
        public string Text { get; set; }
        public StorageItem File { get; set; }
    }
}