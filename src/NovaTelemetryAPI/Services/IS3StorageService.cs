namespace NovaTelemetryAPI.Services
{
    public interface IS3StorageService
    {
        // Uploads raw telemetry data string to the S3 data lake
        Task<bool> UploadTelemetryAsync(string fileName, string content);
    }
}