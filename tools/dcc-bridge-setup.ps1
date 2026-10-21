# DCC Bridge Setup Script for NAAD Project
# Installs and configures dcc-bridge for Blender, Maya, and 3ds Max

Write-Host "=== DCC Bridge Setup for NAAD ===" -ForegroundColor Cyan

# Check if Python is installed
Write-Host "`n[1/4] Checking Python installation..." -ForegroundColor Yellow
try {
    $pythonVersion = python --version 2>&1
    Write-Host "Found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "Python not found. Please install Python 3.8+ first." -ForegroundColor Red
    exit 1
}

# Install dcc-bridge
Write-Host "`n[2/4] Installing dcc-bridge..." -ForegroundColor Yellow
pip install dcc-bridge

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to install dcc-bridge" -ForegroundColor Red
    exit 1
}
Write-Host "dcc-bridge installed successfully" -ForegroundColor Green

# Configure Blender
Write-Host "`n[3/4] Configuring Blender..." -ForegroundColor Yellow
dcc setup blender
Write-Host "Blender configured" -ForegroundColor Green

# Configure Maya (optional - uncomment if needed)
# Write-Host "`nConfiguring Maya 2024..." -ForegroundColor Yellow
# dcc setup maya --version 2024
# Write-Host "Maya 2024 configured" -ForegroundColor Green

# Configure 3ds Max (optional - uncomment if needed)
# Write-Host "`nConfiguring 3ds Max..." -ForegroundColor Yellow
# dcc setup 3dsmax
# Write-Host "3ds Max configured" -ForegroundColor Green

# Verify installation
Write-Host "`n[4/4] Verifying installation..." -ForegroundColor Yellow
dcc status

Write-Host "`n=== Setup Complete ===" -ForegroundColor Cyan
Write-Host "To use dcc-bridge:" -ForegroundColor White
Write-Host "  - Open your DCC application (Blender, Maya, etc.)" -ForegroundColor Gray
Write-Host "  - Run: dcc status" -ForegroundColor Gray
Write-Host "  - Run: dcc run code `"print('hello')`"" -ForegroundColor Gray
Write-Host "  - Run: dcc run file /path/to/script.py" -ForegroundColor Gray
