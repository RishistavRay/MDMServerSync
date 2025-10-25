using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DeviceId { get; set; } = default!;

        public string DeviceName { get; set; } = default!;

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = default!;

        public bool CameraEnabled { get; set; }
        public bool MicEnabled { get; set; }
        public bool LocationEnabled { get; set; }
        public decimal SoftwareVersion { get; set; }
        public DateTime RegisteredAt { get; set; }

        public AppUser User { get; set; } = default!;
    }
}
