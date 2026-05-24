
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
  required_version = ">= 1.5.0"
}

provider "aws" {
  region  = "us-east-1"
  profile = "terraform-executor"
}

# 1. Firewall (Security Group) for Database
resource "aws_security_group" "postgres_sg" {
  name        = "Nova-postgres-sg"
  description = "Access au PostgreSQL"

  # IN: Permettre connexions au Postgres (5432) depuis n'importe quelle adresse IP (lab)
  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    # en prod on permettre que seul le IP de l'API puisse acceder au BD.
  }

  # OUT: Permettre au BD de repondre aux requetes de votre API
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# 2. Instance du BD PostgreSQL (AWS RDS)
resource "aws_db_instance" "postgres_db" {
  identifier            = "Nova-rd-database"
  allocated_storage     = 20
  max_allocated_storage = 100
  engine                = "postgres"
  engine_version        = "16"
  instance_class        = "db.t3.micro" # free tier
  
  db_name              = "Nova_telemetry_db"
  username             = "db_admin_raoni"
  password             = "RaoniNovaProject20261234qq"
  
  vpc_security_group_ids = [aws_security_group.postgres_sg.id]
  publicly_accessible    = true
  skip_final_snapshot    = true
}

# 3. Output: URL de connexion
output "postgres_endpoint" {
  value       = aws_db_instance.postgres_db.endpoint
  description = "Copier cette URL pour configurer votre API et se connecter à la base de données."
}

# ==========================================
# Amazon S3 Bucket for Telemetry Data Lake
# ==========================================

resource "aws_s3_bucket" "telemetry_data_lake" {
  # Name must be globally unique across all AWS accounts
  bucket        = "fentum-telemetry-data-lake-raoni" 
  force_destroy = true # Allows Terraform to delete the bucket even if it contains files during dev

  tags = {
    Name        = "Telemetry Data Lake"
    Environment = "Development"
    Project     = "Nova"
  }
}

# Block all public access to guarantee data privacy
resource "aws_s3_bucket_public_access_block" "telemetry_data_lake_privacy" {
  bucket = aws_s3_bucket.telemetry_data_lake.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}
