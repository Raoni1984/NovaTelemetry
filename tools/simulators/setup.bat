@echo off
echo ===================================================
echo  Nova Telemetry Project - Automated Environment Setup
echo ===================================================

echo.
echo 1/3 Restore .NET API dependencies...
dotnet restore

echo.
echo 2/3 Install Python Simulator dependencies...
pip install requests

echo.
echo 3/3 Verifying Terraform initialization...
terraform init

echo.
echo ===================================================
echo  Setup completed successfully! 
echo  You are ready to run the API, Simulator, and Terraform.
echo ===================================================
pause