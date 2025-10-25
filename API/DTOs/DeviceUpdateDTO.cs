namespace API.DTOs
{
    public class DeviceUpdateDto
    {
        public required string DeviceId { get; set; } // Primary key in Device table
        public bool? CameraEnabled { get; set; }
        public bool? MicEnabled { get; set; }
        public bool? LocationEnabled { get; set; }
        public decimal? SoftwareVersion { get; set; }
    }
}
