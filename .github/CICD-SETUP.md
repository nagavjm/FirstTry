# CI/CD Setup Guide

## 📁 Project Structure

```
FirstTry/
├── .github/
│   └── workflows/
│       ├── build.yml          # CI: Feature branches & PR validation
│       └── deploy.yml         # CD: Main branch deployment
├── Backend/
│   ├── src/
│   │   ├── FirstTry.API/      # ASP.NET Core 8 Web API
│   │   ├── FirstTry.Application/
│   │   ├── FirstTry.Domain/
│   │   └── FirstTry.Infrastructure/
│   └── FirstTry.sln
└── Frontend/                   # Angular Application
    ├── src/
    ├── package.json
    └── angular.json
```

## 🔐 Required GitHub Secrets

Navigate to: **Repository → Settings → Secrets and variables → Actions**

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `OCTOPUS_SERVER_URL` | Octopus Deploy server URL | `https://your-octopus.com` |
| `OCTOPUS_API_KEY` | Octopus Deploy API key | `API-XXXXXXXXXXXX` |
| `OCTOPUS_SPACE` | Octopus Space name | `Default` |

### How to Get Octopus API Key:
1. Login to Octopus Deploy
2. Click your profile → **Profile**
3. Go to **My API Keys**
4. Click **New API Key**
5. Copy and save securely

## 🔒 Branch Protection Rules

Navigate to: **Repository → Settings → Branches → Add rule**

### For `main` / `master` branch:

| Setting | Value |
|---------|-------|
| Branch name pattern | `main` or `master` |
| ✅ Require a pull request before merging | Enabled |
| ✅ Require approvals | 1 (or more) |
| ✅ Require status checks to pass | Enabled |
| ✅ Require branches to be up to date | Enabled |
| Status checks required | `build-validation` |
| ✅ Require conversation resolution | Enabled |
| ✅ Do not allow bypassing settings | Recommended |

## 🐙 Octopus Deploy Configuration

### 1. Create Project
1. Login to Octopus Deploy
2. Go to **Projects → Add Project**
3. Name: `FirstTry`
4. Select Project Group and Lifecycle

### 2. Create Environment
- Development
- Staging  
- Production

### 3. Configure Deployment Process

Add these steps in order:

#### Step 1: Stop IIS Website
```powershell
# Type: Run a Script
Import-Module WebAdministration
$siteName = "FirstTry"
if ((Get-WebSiteState -Name $siteName).Value -eq "Started") {
    Stop-Website -Name $siteName
    Write-Host "Website '$siteName' stopped."
}
```

#### Step 2: Deploy Package
- Type: **Deploy a Package**
- Package: `FirstTry.Web`
- Target: `C:\inetpub\wwwroot\FirstTry`

#### Step 3: Start IIS Website
```powershell
# Type: Run a Script
Import-Module WebAdministration
$siteName = "FirstTry"
Start-Website -Name $siteName
Write-Host "Website '$siteName' started."
```

### 4. Configure Tentacle on IIS Server

#### Install Tentacle:
```powershell
# Download and install Octopus Tentacle
# https://octopus.com/downloads/tentacle

# Register Tentacle with Octopus Server
& "C:\Program Files\Octopus Deploy\Tentacle\Tentacle.exe" `
    register-with `
    --server="https://your-octopus.com" `
    --apiKey="API-XXXXXXXXXXXX" `
    --environment="Production" `
    --role="web-server" `
    --name="IIS-Server-01"
```

## 🌐 IIS Configuration

### 1. Install Prerequisites
```powershell
# Install IIS
Install-WindowsFeature -Name Web-Server -IncludeManagementTools

# Install ASP.NET Core Hosting Bundle
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

### 2. Create IIS Website
```powershell
Import-Module WebAdministration

# Create Application Pool
New-WebAppPool -Name "FirstTryPool"
Set-ItemProperty "IIS:\AppPools\FirstTryPool" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\FirstTryPool" -Name "startMode" -Value "AlwaysRunning"

# Create Website
New-Website -Name "FirstTry" `
    -PhysicalPath "C:\inetpub\wwwroot\FirstTry" `
    -ApplicationPool "FirstTryPool" `
    -Port 80
```

### 3. Configure web.config (auto-generated, but verify):
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" 
             modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\FirstTry.API.dll"
                  stdoutLogEnabled="false" 
                  hostingModel="InProcess" />
    </system.webServer>
  </location>
</configuration>
```

## 🔄 Workflow Summary

| Trigger | Workflow | Actions |
|---------|----------|---------|
| Push to `feature/*` | `build.yml` | Build + Test |
| PR to `main` | `build.yml` | Build + Test + Block merge if fail |
| Merge to `main` | `deploy.yml` | Build + Package + Push to Octopus + Deploy |
| Manual trigger | `deploy.yml` | Same as above with environment selection |

