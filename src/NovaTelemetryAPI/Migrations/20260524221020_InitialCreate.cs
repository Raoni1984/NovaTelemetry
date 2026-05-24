using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaTelemetryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sensor_telemetry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    laser_intensity = table.Column<double>(type: "double precision", nullable: false),
                    temperature = table.Column<double>(type: "double precision", nullable: false),
                    voltage = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensor_telemetry", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_timestamp_equipment",
                table: "sensor_telemetry",
                columns: new[] { "timestamp", "equipment_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sensor_telemetry");
        }
    }
}
