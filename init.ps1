#Requires -Version 7.0
<#
.SYNOPSIS
    Thin wrapper that collects interactive input, then delegates to NUKE Init target.
.EXAMPLE
    ./init.ps1
.EXAMPLE
    ./init.ps1 -ProjectName Acme.Payments -Author "Acme Corp" -Force
#>
param(
    [ValidatePattern('^[A-Za-z][A-Za-z0-9.]*$')]
    [string]$ProjectName,

    [string]$Author,

    [switch]$ResetGit,

    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$ScriptDir = $PSScriptRoot

# ── Interactive wizard ───────────────────────────────────────────────

if (-not $Force) {
    Write-Host ""
    Write-Host "  ChengYuan - Project Setup" -ForegroundColor Cyan
    Write-Host "  =========================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  This wizard will customize the template for your project."
    Write-Host ""

    if (-not $ProjectName) {
        do {
            $ProjectName = Read-Host "  ? Project name (e.g. Acme.Payments)"
            if ($ProjectName -notmatch '^[A-Za-z][A-Za-z0-9.]*$') {
                Write-Host "    Must start with a letter and contain only letters, digits, and dots." -ForegroundColor Red
                $ProjectName = ''
            }
        } while (-not $ProjectName)
    }
    Write-Host "    Project name: $ProjectName" -ForegroundColor Green

    if (-not $Author) {
        $Author = Read-Host "  ? Author / organization (leave blank to skip)"
    }
    if ($Author) {
        Write-Host "    Author: $Author" -ForegroundColor Green
    }

    if (-not $ResetGit) {
        $resetAnswer = Read-Host "  ? Reset git history to a fresh commit? [y/N]"
        $ResetGit = $resetAnswer -match '^[Yy]'
    }

    Write-Host ""
    Write-Host "  WARNING: This operation is IRREVERSIBLE." -ForegroundColor Red
    Write-Host ""

    $confirm = Read-Host "  ? Proceed? [y/N]"
    if ($confirm -notmatch '^[Yy]') {
        Write-Host ""
        Write-Host "  Cancelled." -ForegroundColor Yellow
        exit 0
    }
    Write-Host ""
}
else {
    if (-not $ProjectName) {
        Write-Error "ProjectName is required when using -Force."
        exit 1
    }
}

# ── Delegate to NUKE Init target ─────────────────────────────────────

$buildArgs = @("Init", "--ProjectName", $ProjectName)
if ($Author) {
    $buildArgs += @("--Author", $Author)
}
if ($ResetGit) {
    $buildArgs += "--ResetGit"
}

& "$ScriptDir\build.ps1" @buildArgs
