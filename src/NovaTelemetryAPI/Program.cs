using Amazon.S3;
using NovaTelemetryAPI.Data;
using NovaTelemetryAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Dependency Injection - Infrastructure Configurations
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<TelemetryContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Dependency Injection - Core Framework Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Metadata extraction setup for OpenAPI documentation generation
builder.Services.AddOpenApi();

// Register AWS S3 Client and Custom Storage Service
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IS3StorageService, S3StorageService>();

var app = builder.Build();

// 3. HTTP Request Pipeline Configuration (Middleware Ordering)
if (app.Environment.IsDevelopment())
{
    // Maps the OpenAPI JSON document endpoints
    app.MapOpenApi();
}

// CRITICAL FIX: HTTPS Redirection must run BEFORE authorization and controller mapping
app.UseHttpsRedirection();

app.UseAuthorization();

// Maps attribute-routed API controllers to the pipeline execution context
app.MapControllers();

app.Run();