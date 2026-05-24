# Nova Data Platform

An end-to-end cloud-native IoT telemetry ingestion pipeline built to capture, analyze, and store real-time industrial machinery metrics. The platform utilizes a hybrid lambda architecture (Hot/Cold Path) to efficiently route operational telemetry into a cold storage Data Lake for future machine learning applications, while isolating critical overheating anomalies in a high-performance relational database for real-time alerting.

---

## Architecture Overview

The system processes telemetry batches ingested from distributed laser machinery through an automated simulation system.

1. **The Generator (Python Simulator):** Simulates real-time hardware status, periodically shifting from normal behavior to critical overheating anomalies.
2. **The Ingestion Engine (.NET 8 Core Web API):** Sanitizes and validates data, ensuring proper global timestamps (`UTC`) and `UUID` identifiers.
3. **The Cold Path (Amazon S3 Data Lake):** 100% of the raw JSON batch telemetry is serialized and written to an immutable S3 Bucket partitioned by date (`yyyy/MM/dd`) for long-term data science and computational analytics.
4. **The Hot Path (Amazon RDS PostgreSQL):** Only telemetry rows indicating critical anomalies (Temperature > 200°C) are dynamically filtered and persisted using high-throughput batch insertion for real-time monitoring.

---

## Project Structure

```text
Nova-data-platform/
│
├── .gitignore                  # Prevents committing credentials, binaries, and virtual environments
├── README.md                   # High-level architecture overview and setup guide
│
├── infra/                      # Infrastructure as Code (IaC)
│   ├── .terraform/             # Local directory managed by Terraform init (auto-generated)
│   ├── main.tf                 # AWS Providers, RDS PostgreSQL, S3 Data Lake Bucket, and Security Groups
│   ├── terraform.tfstate       # Tracks the current state of AWS cloud resources
│   └── terraform.tfstate.backup # Automatic backup of the previous cloud state
│
├── src/                        # Backend Application Layer
│   └── NovaTelemetryAPI/
│       ├── Controllers/
│       │   └── TelemetryController.cs  # Hybrid Ingestion Controller (Hot/Cold path router)
│       │
│       ├── Data/
│       │   └── TelemetryContext.cs     # EF Core DbContext with performance configuration
│       │
│       ├── Migrations/                 # EF Core database schema tracking (auto-generated)
│       │   ├── 20260521XXXXXX_InitialCreate.cs
│       │   └── TelemetryContextModelSnapshot.cs
│       │
│       ├── Models/
│       │   └── TelemetryData.cs        # Domain model representing physical hardware sensor outputs
│       │
│       ├── Properties/
│       │   └── launchSettings.json     # Configures local ports, profiles, and development URLs
│       │
│       ├── appsettings.json            # Base system settings
│       ├── appsettings.Development.json # Local environment configuration (AWS resources & DB strings)
│       ├── NovaTelemetryAPI.csproj   # .NET 8 Project dependencies definition file
│       └── Program.cs                  # Bootstrapping, AWS SDK Dependency Injection, and Middleware
│
└── tools/                      # Automation, testing, and simulation scripts
    └── simulator/
        ├── .venv/              # Python isolated execution virtual environment
        ├── requirements.txt    # Python package dependency definitions (e.g., requests)
        └── simulator.py        # Intelligent device telemetry load generator loop script
```


## Tech Stack

* **Backend Engine:** .NET 8 Web API (C#)
* **ORM:** Entity Framework Core (EF Core)
* **Infrastructure:** Terraform (IaC)
* **Cloud Provider:** Amazon Web Services (AWS S3 & AWS RDS)
* **Relational Database:** PostgreSQL
* **Simulation Layer:** Python 3.x (Requests library)

---

## Getting Started

### 1. Infrastructure Provisioning (Terraform)
Navigate to the infrastructure directory, initialize the backend, and apply the architecture plan to AWS:

```bash
cd infra
terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

### 2. Database Schema Migration
Ensure your target connection string is set up in `appsettings.Development.json`. Apply Entity Framework migrations to structure your PostgreSQL tables on AWS RDS:

```bash
cd ../src/NovaTelemetryAPI
dotnet ef database update
```
### 3. Running the Ingestion Engine (.NET API)
Restore dependencies and start the REST API on its development port:

```bash
dotnet restore
dotnet run --urls "http://localhost:5000"
```

### 4. Running the Machine Simulator (Python)
Open a new terminal window, initialize the virtual environment, install package dependencies, and run the streaming client loop:

```bash
cd tools/simulator
python -m venv .venv
source .venv/bin/activate  # On Windows use: .venv\Scripts\activate
pip install -r requirements.txt
python simulator.py
```

### Security and Local Environment Guidelines
CRITICAL SECURITY NOTE: Never commit your actual AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, or database credentials into source control. The .gitignore file has been pre-configured to block appsettings.json and *.tfstate files.

Local authentication with Amazon S3 relies on the AWS SDK fallback chain. If credentials are missing in your local appsettings.json, the backend will automatically pull active session tokens directly from your machine's global secure profile located at ~/.aws/credentials.
