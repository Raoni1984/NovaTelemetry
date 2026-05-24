using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using NovaTelemetryAPI.Data;
using NovaTelemetryAPI.Models;
using NovaTelemetryAPI.Services;

namespace NovaTelemetryAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly TelemetryContext _context;
        private readonly IS3StorageService _s3Service;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(
            TelemetryContext context, 
            IS3StorageService s3Service, 
            ILogger<TelemetryController> logger)
        {
            _context = context;
            _s3Service = s3Service;
            _logger = logger;
        }

        // POST api/v1/telemetry/batch
        [HttpPost("batch")]
        public async Task<IActionResult> IngestBatch([FromBody] List<TelemetryData> telemetryBatch)
        {
            if (telemetryBatch == null || !telemetryBatch.Any())
            {
                return BadRequest("Telemetry batch data cannot be empty.");
            }

            try
            {
                // 1. Data Sanitization & Normalization (Your original mandatory logic)
                foreach (var item in telemetryBatch)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                    }
                    if (item.Timestamp == default)
                    {
                        item.Timestamp = DateTime.UtcNow;
                    }
                    item.Timestamp = DateTime.SpecifyKind(item.Timestamp, DateTimeKind.Utc);
                }

                // 2. Cold Path: Upload the complete sanitized batch to Amazon S3 Data Lake
                string fileName = $"batch_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid()}.json";
                string jsonContent = JsonSerializer.Serialize(telemetryBatch);
                
                bool s3UploadSuccess = await _s3Service.UploadTelemetryAsync(fileName, jsonContent);
                
                if (!s3UploadSuccess)
                {
                    _logger.LogWarning("Telemetry batch processed but failed to back up to S3 Data Lake.");
                }

                // 3. Hot Path: Filter and save ONLY critical anomalies (Overheating > 200°C) to PostgreSQL
                var anomalies = telemetryBatch.Where(t => t.Temperature > 200.0).ToList();
                
                if (anomalies.Any())
                {
                    _logger.LogWarning("Detected {Count} critical anomalies! Routing to PostgreSQL RDS...", anomalies.Count);
                    
                    // High-throughput performance optimization: batch insertion for anomalies
                    await _context.Telemetries.AddRangeAsync(anomalies);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { 
                    TotalIngested = telemetryBatch.Count, 
                    AnomaliesDetected = anomalies.Count,
                    Status = "Success" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during hybrid batch ingestion pipeline execution.");
                return StatusCode(500, "Internal server error during data ingestion.");
            }
        }

        // GET api/v1/telemetry/latest
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestTelemetry()
        {
            try
            {
                var data = await _context.Telemetries
                    .OrderByDescending(t => t.Timestamp)
                    .Take(100)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving telemetry data from AWS.");
                return StatusCode(500, "Internal server error while fetching data.");
            }
        }
    }
}