using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Photos
{
    public class SimpleImageInfo
    {
        public string FileName { get; set; }
        public string Info { get; set; }
        public StorageFile Source { get; set; }
        public StorageItemThumbnail Thumbnail { get; set; }
        public int Id { get; set; }
    }
}