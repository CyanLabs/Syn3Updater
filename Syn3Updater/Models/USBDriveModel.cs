namespace Syn3Updater.Models
{
    public class USBDriveModel
    {
        public class Drive
        {
            public string Path { get; set; }
            public string? Name { get; set; }
            public string Size { get; set; }
            public string? Letter { get; set; }
            public string FileSystem { get; set; }
            public string PartitionType { get; set; }
            public string FreeSpace { get; set; }
            public bool SkipFormat { get; set; }
            public string? VolumeName { get; set; }
            public string? Model { get; set; }
            public bool Fake { get; set; }
            public bool Encrypted { get; set; }
            public string EncryptionStatus { get; set; }
            
        }
    }
}