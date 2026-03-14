#Requires -Version 7.0
<#
.SYNOPSIS
    Initializes this template with your project name via interactive wizard.
.DESCRIPTION
    Run without parameters for an interactive guided setup.
    Supply -ProjectName and -Force for non-interactive (CI/script) usage.
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

$OldSample = 'Dotnet.CI.Template.Sample'
$OldSln    = 'Dotnet.CI.Template'
$ScriptDir = $PSScriptRoot

# ── Interactive wizard ───────────────────────────────────────────────

if (-not $Force) {
    Write-Host ""
    Write-Host "  dotnet.CI.template - Project Setup" -ForegroundColor Cyan
    Write-Host "  ===================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  This wizard will customize the template for your project."
    Write-Host ""

    # Project name
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

    # Author
    if (-not $Author) {
        $Author = Read-Host "  ? Author / organization (leave blank to skip)"
    }
    if ($Author) {
        Write-Host "    Author: $Author" -ForegroundColor Green
    }

    # Reset git
    if (-not $ResetGit) {
        $resetAnswer = Read-Host "  ? Reset git history to a fresh commit? [y/N]"
        $ResetGit = $resetAnswer -match '^[Yy]'
    }

    # Preview
    $affectedFiles = Get-ChildItem -Path $ScriptDir -Recurse -File |
        Where-Object {
            $path = $_.FullName
            $skip = $false
            foreach ($d in @('.git', 'node_modules', 'artifacts', 'dist', 'bin', 'obj')) {
                if ($path -match [regex]::Escape([IO.Path]::DirectorySeparatorChar + $d + [IO.Path]::DirectorySeparatorChar)) {
                    $skip = $true; break
                }
            }
            if (-not $skip -and $_.Name -notin @('init.sh', 'init.ps1')) {
                $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
                $content -and ($content -match [regex]::Escape($OldSample) -or $content -match [regex]::Escape($OldSln))
            }
        }
    $affectedDirs = Get-ChildItem -Path $ScriptDir -Recurse -Directory |
        Where-Object { $_.Name -match [regex]::Escape($OldSample) -and $_.FullName -notmatch '\.git' }

    $currentVersion = '(unknown)'
    $propsPath = Join-Path $ScriptDir 'Directory.Build.props'
    if (Test-Path $propsPath) {
        $propsContent = Get-Content $propsPath -Raw
        if ($propsContent -match '<VersionPrefix>([^<]+)</VersionPrefix>') {
            $currentVersion = $Matches[1]
        }
    }

    Write-Host ""
    Write-Host "  ──────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  The following changes will be applied:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "    Rename   $OldSample -> $ProjectName"
    Write-Host "    Rename   $OldSln.slnx -> $ProjectName.slnx"
    if ($Author) {
        Write-Host "    Update   Authors -> $Author"
    }
    Write-Host "    Reset    VersionPrefix $currentVersion -> 0.1.0"
    Write-Host "    Git      $(if ($ResetGit) { 'reset to fresh commit' } else { 'preserved' })"
    Write-Host "    Affect   $($affectedFiles.Count) files, $($affectedDirs.Count) directories"
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

# ── Execute ──────────────────────────────────────────────────────────

Write-Host "[1/6] Replacing file contents..."
$excludeDirs = @('.git', 'node_modules', 'artifacts', 'dist', 'bin', 'obj')
Get-ChildItem -Path $ScriptDir -Recurse -File |
    Where-Object {
        $path = $_.FullName
        $skip = $false
        foreach ($d in $excludeDirs) {
            if ($path -match [regex]::Escape([IO.Path]::DirectorySeparatorChar + $d + [IO.Path]::DirectorySeparatorChar)) {
                $skip = $true; break
            }
        }
        -not $skip -and $_.Name -notin @('init.sh', 'init.ps1')
    } |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
        if ($content -and ($content -match [regex]::Escape($OldSample) -or $content -match [regex]::Escape($OldSln))) {
            $content = $content -replace [regex]::Escape($OldSample), $ProjectName
            $content = $content -replace [regex]::Escape($OldSln), $ProjectName
            Set-Content $_.FullName -Value $content -NoNewline
        }
    }

# Author update in csproj
if ($Author) {
    Write-Host "[2/6] Updating author..."
    Get-ChildItem -Path $ScriptDir -Recurse -File -Filter '*.csproj' |
        Where-Object { $_.FullName -notmatch '[\\/](_build|build)[\\/]' } |
        ForEach-Object {
            $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
            if ($content -and $content -match '<Authors>[^<]*</Authors>') {
                $content = $content -replace '<Authors>[^<]*</Authors>', "<Authors>$Author</Authors>"
                Set-Content $_.FullName -Value $content -NoNewline
            }
        }
}
else {
    Write-Host "[2/6] Skipping author update..."
}

# Rename directories (deepest first)
Write-Host "[3/6] Renaming directories..."
Get-ChildItem -Path $ScriptDir -Recurse -Directory |
    Where-Object { $_.Name -match [regex]::Escape($OldSample) -and $_.FullName -notmatch '\.git' } |
    Sort-Object { $_.FullName.Length } -Descending |
    ForEach-Object {
        $newName = $_.Name -replace [regex]::Escape($OldSample), $ProjectName
        $newPath = Join-Path $_.Parent.FullName $newName
        Write-Host "  $($_.FullName) -> $newPath"
        Rename-Item $_.FullName $newPath
    }

# Rename files
Write-Host "[4/6] Renaming files..."
Get-ChildItem -Path $ScriptDir -Recurse -File |
    Where-Object { $_.Name -match [regex]::Escape($OldSample) -and $_.FullName -notmatch '\.git' } |
    ForEach-Object {
        $newName = $_.Name -replace [regex]::Escape($OldSample), $ProjectName
        $newPath = Join-Path $_.Directory.FullName $newName
        Write-Host "  $($_.FullName) -> $newPath"
        Rename-Item $_.FullName $newPath
    }

$slnFile = Join-Path $ScriptDir "$OldSln.slnx"
if (Test-Path $slnFile) {
    $newSlnFile = Join-Path $ScriptDir "$ProjectName.slnx"
    Rename-Item $slnFile $newSlnFile
    Write-Host "  $OldSln.slnx -> $ProjectName.slnx"
}

# Reset version
Write-Host "[5/6] Resetting version to 0.1.0..."
$propsFile = Join-Path $ScriptDir 'Directory.Build.props'
if (Test-Path $propsFile) {
    $xml = Get-Content $propsFile -Raw
    $xml = $xml -replace '<VersionPrefix>[^<]*</VersionPrefix>', '<VersionPrefix>0.1.0</VersionPrefix>'
    Set-Content $propsFile -Value $xml -NoNewline
}

# Update build paths
Write-Host "[6/6] Updating build configuration..."
$paramsFile = Join-Path $ScriptDir 'build/BuildTask.Parameters.cs'
if (Test-Path $paramsFile) {
    $cs = Get-Content $paramsFile -Raw
    $cs = $cs -replace [regex]::Escape("$OldSln.slnx"), "$ProjectName.slnx"
    Set-Content $paramsFile -Value $cs -NoNewline
}

# Optional: reset git history
if ($ResetGit) {
    Write-Host ""
    Write-Host "Resetting git history..."
    Remove-Item (Join-Path $ScriptDir '.git') -Recurse -Force
    git -C $ScriptDir init
    git -C $ScriptDir add .
    git -C $ScriptDir commit -m "Initial commit from dotnet.CI.template"
}

# Clean up init scripts
Remove-Item (Join-Path $ScriptDir 'init.sh') -ErrorAction SilentlyContinue
Remove-Item (Join-Path $ScriptDir 'init.ps1') -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Done! Your project '$ProjectName' is ready." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Update GitHub URL in docs/.vitepress/config.ts"
Write-Host "  2. Run: dotnet restore --force-evaluate"
Write-Host "  3. Run: dotnet build"
