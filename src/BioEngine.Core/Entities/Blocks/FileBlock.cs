namespace BioEngine.Core.Entities.Blocks
{
    public class FileBlock : ContentBlock<FileBlockData>
    {
        public override string TypeTitle { get; set; } = "Файл";
    }

    public class FileBlockData : ContentBlockData
    {
        public StorageItem File { get; set; }
    }
}
