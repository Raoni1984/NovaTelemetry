using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NovaTelemetryAPI.Services
{
    public class S3StorageService : IS3StorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<S3StorageService> _logger;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3StorageService> logger)
        {
            _s3Client = s3Client;
            // Matches the bucket name defined in your main.tf
            _bucketName = configuration["AWS:S3BucketName"] ?? "fentum-telemetry-data-lake-raoni";
            _logger = logger;
        }

        public async Task<bool> UploadTelemetryAsync(string fileName, string content)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = $"raw-zone/{DateTime.UtcNow:yyyy/MM/dd}/{fileName}",
                    ContentBody = content,
                    ContentType = "application/json"
                };

                var response = await _s3Client.PutObjectAsync(putRequest);
                
                _logger.LogInformation("Successfully uploaded {FileName} to S3 bucket {Bucket}.", fileName, _bucketName);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 specific error occurred while uploading {FileName}.", fileName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while uploading {FileName} to S3.", fileName);
                return false;
            }
        }
    }
}