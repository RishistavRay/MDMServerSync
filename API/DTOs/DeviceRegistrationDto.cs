namespace API.DTOs
{
    public class DeviceRegistrationDto
    {
        public string DeviceId { get; set; } = default!;
        public string DeviceName { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public bool CameraEnabled { get; set; }
        public bool MicEnabled { get; set; }
        public bool LocationEnabled { get; set; }
        public decimal SoftwareVersion { get; set; }
    }
}