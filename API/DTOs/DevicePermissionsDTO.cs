namespace API.DTOs
{
    public class DevicePermissionsDto
    {
        public string Id { get; set; } = string.Empty;
        public bool CameraEnabled { get; set; }
        public bool MicEnabled { get; set; }
        public bool LocationEnabled { get; set; }
        public decimal SoftwareVersion { get; set; }
    }
}
