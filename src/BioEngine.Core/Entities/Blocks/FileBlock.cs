using BioEngine.Core.DB;

namespace BioEngine.Core.Entities.Blocks
{
    [TypedEntity("file")]
    public class FileBlock : ContentBlock<FileBlockData>
    {
        public override string TypeTitle { get; set; } = "Файл";

        public override string ToString()
        {
            return $"Файл: {Data.File.FileName}";
        }
    }

    public class FileBlockData : ContentBlockData
    {
        public StorageItem File { get; set; }
    }
}
