namespace Syn3Updater.Models.Mac
{
    public class DiskUtilModel
    {
        public class DiskUtilInfo
        {
            public string? DeviceIdentifier { get; set; }
            public string? DeviceNode { get; set; }
            public bool? Whole { get; set; }
            public string? PartOfWhole { get; set; }
            public string? VolumeName { get; set; }
            public bool? Mounted { get; set; }
            public string? MountPoint { get; set; }
            public string? PartitionType { get; set; }
            public string? FileSystemPersonality { get; set; }
            public string? TypeBundle { get; set; }
            public string? NameUserVisible { get; set; }
            public bool? OsCanBeInstalled { get; set; }
            public string? MediaType { get; set; }
            public string? Protocol { get; set; }
            public string? SmartStatus { get; set; }
            public string? VolumeUuid { get; set; }
            public string? PartitionOffset { get; set; }
            public string? DiskSize { get; set; }
            public string? DeviceBlockSize { get; set; }
            public string? VolumeTotalSpace { get; set; }
            public string? VolumeUsedSpace { get; set; }
            public string? VolumeFreeSpace { get; set; }
            public string? AllocationBlockSize { get; set; }
            public bool? MediaOsUseOnly { get; set; }
            public bool? MediaReadOnly { get; set; }
            public bool? VolumeReadOnly { get; set; }
            public string? DeviceLocation { get; set; }
            public string? RemovableMedia { get; set; }
            public string? MediaRemoval { get; set; }
            public string? SolidState { get; set; }
        }
    }
}