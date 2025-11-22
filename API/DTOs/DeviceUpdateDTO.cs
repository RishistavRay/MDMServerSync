using System.Text.Json.Serialization;

namespace API.DTOs
{
    public class DeviceUpdateDto
    {
        public required string DeviceId { get; set; } // Primary key in Device table
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? CameraEnabled { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? MicEnabled { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? LocationEnabled { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? SoftwareVersion { get; set; }
    }
}
