# Apply Modeling/Execution split — waves B–D (host rename + first model cutover).
param([switch]$WhatIf)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

$namespaceMap = @{
    'AIGuiders.Platform.Abstractions' = 'AIGuiders.Platform.Modeling.Core'
    'AIGuiders.Platform.Paths' = 'AIGuiders.Platform.Modeling.Paths'
    'AIGuiders.Platform.Catalog' = 'AIGuiders.Platform.Modeling.Catalog'
    'AIGuiders.Platform.IntermediateRepresentation.Bracket' = 'AIGuiders.Platform.Modeling.Notations.Bracket'
    'AIGuiders.Platform.IntermediateRepresentation.Keyboard' = 'AIGuiders.Platform.Modeling.Notations.Keyboard'
    'AIGuiders.Platform.Execution.Cockpit.Cds' = 'AIGuiders.Platform.Modeling.Cockpit.Cds'
}

$removeProjects = @(
    'src/AIGuiders.Platform.Abstractions',
    'src/AIGuiders.Platform.Paths',
    'src/AIGuiders.Platform.Catalog',
    'src/AIGuiders.Platform.IntermediateRepresentation.Bracket',
    'src/AIGuiders.Platform.IntermediateRepresentation.Keyboard'
)

$projectRefToFlag = @{
    'AIGuiders.Platform.Abstractions.csproj' = 'UseGuidersModelingCore'
    'AIGuiders.Platform.Paths.csproj' = 'UseGuidersModelingPaths'
    'AIGuiders.Platform.Catalog.csproj' = 'UseGuidersModelingCatalog'
    'AIGuiders.Platform.IntermediateRepresentation.Bracket.csproj' = 'UseGuidersModelingNotationsBracket'
    'AIGuiders.Platform.IntermediateRepresentation.Keyboard.csproj' = 'UseGuidersModelingNotationsKeyboard'
}

# Execution host families only — NOT Authoring/IR/Notations (→ F# Modeling).
$executionRenameFamilies = @(
    'CommandPlane', 'Sources', 'Configurations', 'LanguageIntelligence', 'Language',
    'Documentation', 'Utilities', 'MCPlane', 'Routing'
)

function Replace-InFiles([string[]]$paths, [hashtable]$map) {
    foreach ($file in $paths) {
        $text = [IO.File]::ReadAllText($file)
        $orig = $text
        foreach ($k in $map.Keys) { $text = $text.Replace($k, $map[$k]) }
        if ($text -ne $orig) {
            if ($WhatIf) { Write-Host "Would update $file" }
            else { [IO.File]::WriteAllText($file, $text) }
        }
    }
}

function Get-RelativeImportToEng([string]$csprojPath) {
    $dir = Split-Path $csprojPath -Parent
    $eng = Join-Path $root 'eng\Guiders.Modeling.props'
    $rel = [IO.Path]::GetRelativePath($dir, $eng) -replace '\\', '/'
    return $rel
}

$textFiles = Get-ChildItem -Recurse -Include *.cs,*.csproj,*.slnx,*.md,*.toml -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

Replace-InFiles $textFiles.FullName $namespaceMap

foreach ($rel in $removeProjects) {
    $path = Join-Path $root $rel
    if (Test-Path $path) {
        if ($WhatIf) { Write-Host "Would remove $rel" }
        else { Remove-Item $path -Recurse -Force }
    }
}

$csprojs = Get-ChildItem -Recurse -Filter *.csproj -File | Where-Object { $_.FullName -notmatch '\\obj\\' }
foreach ($csproj in $csprojs) {
    $text = [IO.File]::ReadAllText($csproj.FullName)
    $orig = $text
    $flags = [System.Collections.Generic.HashSet[string]]::new()

    foreach ($entry in $projectRefToFlag.GetEnumerator()) {
        $pattern = "(?s)<ProjectReference[^>]*\\$([regex]::Escape($entry.Key))`"[^>]*/>\s*"
        if ($text -match $pattern) {
            $text = [regex]::Replace($text, $pattern, '')
            [void]$flags.Add($entry.Value)
        }
    }

    if ($flags.Count -gt 0) {
        $importRel = Get-RelativeImportToEng $csproj.FullName
        if ($text -notmatch 'Guiders\.Modeling\.props') {
            $text = $text -replace '</Project>', "  <Import Project=`"$importRel`" />`r`n</Project>"
        }
        foreach ($flag in $flags) {
            if ($text -notmatch "<$flag>") {
                if ($text -match '<PropertyGroup>') {
                    $text = [regex]::Replace($text, '<PropertyGroup>', "<PropertyGroup>`r`n    <$flag>true</$flag>", 1)
                } else {
                    $text = $text -replace '(<Project[^>]*>)', "`$1`r`n  <PropertyGroup>`r`n    <$flag>true</$flag>`r`n  </PropertyGroup>"
                }
            }
        }
    }

    if ($text -ne $orig) {
        if ($WhatIf) { Write-Host "Would patch $($csproj.Name)" }
        else { [IO.File]::WriteAllText($csproj.FullName, $text) }
    }
}

$src = Join-Path $root 'src'
foreach ($family in $executionRenameFamilies) {
    Get-ChildItem $src -Directory -Filter "AIGuiders.Platform.$family*" | ForEach-Object {
        $newName = $_.Name -replace '^AIGuiders\.Platform\.', 'AIGuiders.Platform.Execution.'
        if ($_.Name -eq $newName) { return }
        $newPath = Join-Path $src $newName
        if ($WhatIf) { Write-Host "Would rename $($_.Name) -> $newName" }
        elseif (-not (Test-Path $newPath)) { Rename-Item $_.FullName $newName }
    }
}

$afterFiles = Get-ChildItem -Recurse -Include *.cs,*.csproj,*.slnx,*.md,*.toml -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
foreach ($family in $executionRenameFamilies) {
    $map = @{ "AIGuiders.Platform.$family" = "AIGuiders.Platform.Execution.$family" }
    Replace-InFiles $afterFiles.FullName $map
}

# Cockpit.Cds → F# types only
$cdsDir = Join-Path $src 'AIGuiders.Platform.Execution.Cockpit.Cds'
if (Test-Path $cdsDir) {
    Get-ChildItem $cdsDir -Filter *.cs -ErrorAction SilentlyContinue | ForEach-Object {
        if ($WhatIf) { Write-Host "Would delete $($_.Name)" } else { Remove-Item $_.FullName -Force }
    }
    $cdsProj = Join-Path $cdsDir 'AIGuiders.Platform.Execution.Cockpit.Cds.csproj'
    if (Test-Path $cdsProj) {
        $text = [IO.File]::ReadAllText($cdsProj)
        if ($text -notmatch 'UseGuidersModelingCockpitCds') {
            $importRel = Get-RelativeImportToEng $cdsProj
            $text = $text -replace '(<PropertyGroup>)', "`$1`r`n    <UseGuidersModelingCockpitCds>true</UseGuidersModelingCockpitCds>"
            if ($text -notmatch 'Guiders.Modeling.props') {
                $text = $text -replace '</Project>', "  <Import Project=`"$importRel`" />`r`n</Project>"
            }
            if (-not $WhatIf) { [IO.File]::WriteAllText($cdsProj, $text) }
        }
    }
}

# Routing: model algebra → F#, seam stays Execution
$routingDir = Join-Path $src 'AIGuiders.Platform.Execution.Routing'
if (Test-Path $routingDir) {
    $rf = Join-Path $routingDir 'RouteRefusal.cs'
    if (Test-Path $rf) { if ($WhatIf) { Write-Host 'Would delete RouteRefusal.cs' } else { Remove-Item $rf -Force } }
    $routingProj = Join-Path $routingDir 'AIGuiders.Platform.Execution.Routing.csproj'
    if (Test-Path $routingProj) {
        $text = [IO.File]::ReadAllText($routingProj)
        if ($text -notmatch 'UseGuidersModelingCore') {
            $importRel = Get-RelativeImportToEng $routingProj
            $text = $text -replace '(<PropertyGroup>)', "`$1`r`n    <UseGuidersModelingCore>true</UseGuidersModelingCore>`r`n    <UseGuidersModelingRouting>true</UseGuidersModelingRouting>"
            if ($text -notmatch 'Guiders.Modeling.props') {
                $text = $text -replace '</Project>', "  <Import Project=`"$importRel`" />`r`n</Project>"
            }
            $text = $text -replace 'AIGuiders\.Platform\.Abstractions', 'AIGuiders.Platform.Modeling.Core'
            if (-not $WhatIf) { [IO.File]::WriteAllText($routingProj, $text) }
        }
    }
}

Write-Host 'Done.'
