# Setup bpy (Blender as Python module) for NAAD on Windows.
# Usage: .\tools\blender-bpy\setup-bpy.ps1 [-PythonVersion 3.13] [-BpyVersion ""]
# Creates .venv-bpy, installs matching bpy wheel, runs validate_bpy.py.
# Current bpy (5.x) needs Python 3.13. For archived bpy 4.2 LTS on
# Python 3.11, pass: -PythonVersion 3.11 -BpyVersion "4.2.*" -ExtraIndexUrl "https://download.blender.org/pypi/"
param([string]$PythonVersion = "3.13", [string]$BpyVersion = "", [string]$ExtraIndexUrl = "")
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not $PSScriptRoot) { $Root = Get-Location }
$Venv = Join-Path $Root ".venv-bpy"
$PyTag = "-$PythonVersion"
try { & py $PyTag --version } catch {
  Write-Host "Python $PythonVersion not found (py launcher). Install it from python.org." -ForegroundColor Red
  exit 1
}
$VenvPython = Join-Path $Venv "Scripts\python.exe"
if (-not (Test-Path $VenvPython)) {
  if (Test-Path $Venv) {
    Write-Host "Removing stale venv at $Venv (wrong Python or incomplete)..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $Venv
  }
  & py $PyTag -m venv $Venv
}
& "$Venv\Scripts\pip.exe" install --upgrade pip
$Target = if ($BpyVersion) { "bpy==$BpyVersion" } else { "bpy" }
$PipArgs = @("install", $Target)
if ($ExtraIndexUrl) { $PipArgs += @("--extra-index-url", $ExtraIndexUrl) }
try {
  & "$Venv\Scripts\pip.exe" @PipArgs
} catch {
  Write-Host "pip install $Target failed. Likely no wheel for Python $PythonVersion." -ForegroundColor Red
  Write-Host "Fix: install Python 3.13 and use -PythonVersion 3.13, or for bpy 4.2 LTS on 3.11 add -ExtraIndexUrl https://download.blender.org/pypi/ ;" -ForegroundColor Yellow
  Write-Host "or skip bpy and run headless Blender instead:" -ForegroundColor Yellow
  Write-Host "  blender --background --python tools\blender-bpy\validate_bpy.py" -ForegroundColor Yellow
  exit 1
}
& "$Venv\Scripts\python.exe" (Join-Path $PSScriptRoot "validate_bpy.py")
Write-Host "Done. Activate with: .\.venv-bpy\Scripts\Activate.ps1" -ForegroundColor Green

