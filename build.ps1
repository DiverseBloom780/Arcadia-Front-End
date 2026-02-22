# Arcadia Build Script
# This script builds the Arcadia solution and packages it for distribution

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "$PSScriptRoot\..\Build",
    [switch]$Package = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Arcadia Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set paths
$SolutionPath = "$PSScriptRoot\..\Arcadia.sln"
$PublishPath = Join-Path $OutputPath "Publish"
$PackagePath = Join-Path $OutputPath "Package"

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null
New-Item -ItemType Directory -Path $PublishPath | Out-Null

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $SolutionPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore NuGet packages" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "Building solution in $Configuration mode..." -ForegroundColor Yellow
dotnet build $SolutionPath --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed" -ForegroundColor Red
    exit 1
}

# Publish application
Write-Host "Publishing application..." -ForegroundColor Yellow
dotnet publish "..\Source\Arcadia.UI\Arcadia.UI.csproj" `
    --configuration $Configuration `
    --output $PublishPath `
    --self-contained true `
    --runtime win-x64 `
    /p:PublishSingleFile=false `
    /p:IncludeNativeLibrariesForSelfExtract=true
if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed" -ForegroundColor Red
    exit 1
}

# Copy additional files
Write-Host "Copying additional files..." -ForegroundColor Yellow
Copy-Item -Path "..\Config" -Destination $PublishPath -Recurse -Force
Copy-Item -Path "..\Assets" -Destination $PublishPath -Recurse -Force
Copy-Item -Path "..\README.md" -Destination $PublishPath -Force

# Create package if requested
if ($Package) {
    Write-Host "Creating distribution package..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $PackagePath | Out-Null
    
    $Version = "1.0.0"
    $PackageName = "Arcadia-v$Version-Windows-x64.zip"
    $PackageFullPath = Join-Path $PackagePath $PackageName
    
    Compress-Archive -Path "$PublishPath\*" -DestinationPath $PackageFullPath -Force
    
    Write-Host "Package created: $PackageFullPath" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Build completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Output: $PublishPath" -ForegroundColor Cyan
