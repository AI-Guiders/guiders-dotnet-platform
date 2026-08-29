#Requires -Version 7.0
<#
.SYNOPSIS
  Audit AIGuiders.Platform.* NuGet packages and open Manage Owners pages.

.DESCRIPTION
  nuget.org has NO public API to add/remove package owners (only push/delete packages).
  Owner changes require the web UI (or browser session automation).
  This script lists packages, checks current owners via Search API, and optionally
  opens Manage Owners URLs for batch editing.

.PARAMETER OrgUsername
  NuGet.org username of your organization (not GitHub org name).

.PARAMETER OpenManagePages
  Open https://www.nuget.org/packages/{id}/Manage/Owners in default browser.

.EXAMPLE
  ./Add-OrgPackageOwners.ps1 -OrgUsername AIGuiders -OpenManagePages

.NOTES
  After org is co-owner on all packages:
  1. Create org API key (Package Owner = org) for CI.
  2. Update release.yml Trusted Publishing / push identity.
  3. Optionally remove personal owner from each package.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $OrgUsername,

    [switch] $OpenManagePages,

    [string] $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
)

$ErrorActionPreference = 'Stop'

function Get-RepoPackageIds {
    param([string] $Root)
    Get-ChildItem -Path (Join-Path $Root 'src') -Filter '*.csproj' -Recurse |
        ForEach-Object {
            $text = Get-Content -LiteralPath $_.FullName -Raw
            if ($text -match '<PackageId>([^<]+)</PackageId>') {
                $Matches[1].Trim()
            }
        } |
        Sort-Object -Unique
}

function Get-NuGetPackageOwners {
    param([string] $PackageId)
    $q = [uri]::EscapeDataString("packageid:$PackageId")
    $url = "https://azuresearch-usnc.nuget.org/query?q=$q&take=1&prerelease=true&semVerLevel=2.0.0"
    $resp = Invoke-RestMethod -Uri $url -Method Get
    if (-not $resp.data -or $resp.data.Count -eq 0) {
        return $null
    }
    $row = $resp.data[0]
    if ($row.id -ne $PackageId) {
        return $null
    }
    return [pscustomobject]@{
        Id = $row.id
        Version = $row.version
        Owners = @($row.owners -split ',\s*' | Where-Object { $_ })
    }
}

$packageIds = Get-RepoPackageIds -Root $RepoRoot
Write-Host "Found $($packageIds.Count) PackageId entries in src/." -ForegroundColor Cyan
Write-Host "Target org: $OrgUsername" -ForegroundColor Cyan
Write-Host ""
Write-Host "NOTE: nuget.org has no supported owner-management API. Use Manage Owners UI per package." -ForegroundColor Yellow
Write-Host ""

$missing = @()
$pending = @()
$done = @()

foreach ($id in $packageIds) {
    $info = Get-NuGetPackageOwners -PackageId $id
    $manageUrl = "https://www.nuget.org/packages/$id/Edit"

    if (-not $info) {
        Write-Host "[NOT PUBLISHED] $id" -ForegroundColor DarkGray
        continue
    }

    $owners = $info.Owners
    $hasOrg = $owners -contains $OrgUsername

    if ($hasOrg) {
        Write-Host "[OK] $id  owners: $($owners -join ', ')" -ForegroundColor Green
        $done += $id
    }
    else {
        Write-Host "[ADD] $id  owners: $($owners -join ', ')  -> add $OrgUsername" -ForegroundColor Yellow
        Write-Host "      $manageUrl" -ForegroundColor DarkGray
        $pending += [pscustomobject]@{ Id = $id; Url = $manageUrl; Owners = ($owners -join ', ') }
    }

    if ($OpenManagePages -and -not $hasOrg) {
        Start-Process $manageUrl
        Start-Sleep -Milliseconds 400
    }
}

Write-Host ""
Write-Host "Summary: $($done.Count) already have org; $($pending.Count) need Add owner." -ForegroundColor Cyan

if ($pending.Count -gt 0) {
    Write-Host ""
    Write-Host "UI steps (per package):" -ForegroundColor Cyan
    Write-Host "  Manage package -> Owners -> Add owner -> $OrgUsername -> Add"
    Write-Host "  nuget.org UI does NOT trim spaces — paste owner without leading/trailing whitespace."
    Write-Host "  If org: accept invite under org account (Manage Organizations -> pending requests)."
    Write-Host ""
    Write-Host "Bulk tip: run with -OpenManagePages (opens Edit tabs). Add org on each, then accept as org admin."
}
