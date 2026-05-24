using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovaTelemetryAPI.Models
{
    [Table("sensor_telemetry")]
    public class TelemetryData
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("equipment_id")]
        [StringLength(50)]
        public string EquipmentId { get; set; } = string.Empty;

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("laser_intensity")]
        public double LaserIntensity { get; set; }

        [Column("temperature")]
        public double Temperature { get; set; }

        [Column("voltage")]
        public double Voltage { get; set; }
    }
}