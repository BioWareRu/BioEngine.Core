namespace BioEngine.Core.Entities.Blocks
{
    public class FileBlock : PostBlock<FileBlockData>
    {
        public override string TypeTitle { get; set; } = "Файл";
    }

    public class FileBlockData : ContentBlockData
    {
        public StorageItem File { get; set; }
    }
}
