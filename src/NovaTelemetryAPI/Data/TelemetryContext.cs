using NovaTelemetryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace NovaTelemetryAPI.Data
{
    public class TelemetryContext : DbContext
    {
        public TelemetryContext(DbContextOptions<TelemetryContext> options) : base(options) { }

        public DbSet<TelemetryData> Telemetries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // HIGH VOLUME OPTIMIZATION (GB/day):
            // Creating a composite index on Timestamp and EquipmentId.
            // This drastically accelerates time-series analytical queries performed by downstream Data Science/AI scripts.
            modelBuilder.Entity<TelemetryData>()
                .HasIndex(t => new { t.Timestamp, t.EquipmentId })
                .HasDatabaseName("IX_telemetry_timestamp_equipment");
        }
    }
}