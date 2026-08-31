#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$src = Join-Path $root 'src'

function New-IrCsproj($name, $desc) {
    $dir = Join-Path $src "AIGuiders.Platform.IntermediateRepresentation.$name"
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>AIGuiders.Platform.IntermediateRepresentation.$name</RootNamespace>
    <AssemblyName>AIGuiders.Platform.IntermediateRepresentation.$name</AssemblyName>
    <PackageId>AIGuiders.Platform.IntermediateRepresentation.$name</PackageId>
    <Description>$desc (GUIDERS-ADR-0042).</Description>
  </PropertyGroup>
</Project>
"@ | ForEach-Object { Write-Utf8 (Join-Path $dir "AIGuiders.Platform.IntermediateRepresentation.$name.csproj") $_ }
    return $dir
}

function Write-Utf8($file, $content) {
    [System.IO.File]::WriteAllText($file, $content)
}

function Set-Namespace($file, $ns) {
    $c = Get-Content $file -Raw
    $c = $c -replace 'namespace\s+[\w\.]+;', "namespace $ns;"
    Write-Utf8 $file $c
}

function Add-ProjectRef($csproj, $relRef) {
    [xml]$xml = Get-Content $csproj
    $ns = $xml.Project.ItemGroup
    if (-not $ns) { $ig = $xml.CreateElement('ItemGroup'); $xml.Project.AppendChild($ig) | Out-Null }
    $groups = @($xml.Project.ItemGroup)
    $ig = $groups | Where-Object { $_.ProjectReference } | Select-Object -First 1
    if (-not $ig) {
        $ig = $xml.CreateElement('ItemGroup')
        $xml.Project.AppendChild($ig) | Out-Null
    }
    $full = (Resolve-Path (Join-Path (Split-Path $csproj) $relRef)).Path
    foreach ($existing in $ig.ProjectReference) {
        if ($existing.Include -and (Resolve-Path (Join-Path (Split-Path $csproj) $existing.Include) -ErrorAction SilentlyContinue).Path -eq $full) { return }
    }
    $pr = $xml.CreateElement('ProjectReference')
    $pr.SetAttribute('Include', $relRef)
    $ig.AppendChild($pr) | Out-Null
    $xml.Save($csproj)
}

function Replace-ProjectRef($csproj, $oldPattern, $newRel) {
    $text = Get-Content $csproj -Raw
    if ($text -notmatch [regex]::Escape($oldPattern)) { return }
    $text = $text -replace [regex]::Escape($oldPattern), $newRel
    Write-Utf8 $csproj $text
}

function Ensure-Using($file, $using) {
    $c = Get-Content $file -Raw
    if ($c -match [regex]::Escape($using)) { return }
    if ($c -match '(?m)^using ') {
        $c = $c -replace '(?m)(^using [^;]+;\r?\n)', "`$1using $using;`n", 1
    } else {
        $c = "using $using;`n$c"
    }
    Write-Utf8 $file $c
}

# --- Create IR packages ---
$irArg = New-IrCsproj 'Argument' 'Argument IR: profile, slots, NormalizedArguments'
$irKb = New-IrCsproj 'Keyboard' 'Keyboard IR: NormalizedKeySequence'
$irInv = New-IrCsproj 'Invocation' 'Invocation IR: NormalizedCommandLine'
$irBr = New-IrCsproj 'Bracket' 'Bracket wire IR'
$irCmd = New-IrCsproj 'Command' 'Command catalog IR'
$irBind = New-IrCsproj 'Binding' 'Binding catalog IR'
$irMel = New-IrCsproj 'Melody' 'Melody catalog IR'

Add-ProjectRef (Join-Path $irCmd "AIGuiders.Platform.IntermediateRepresentation.Command.csproj") '..\AIGuiders.Platform.IntermediateRepresentation.Argument\AIGuiders.Platform.IntermediateRepresentation.Argument.csproj'
Add-ProjectRef (Join-Path $irBind "AIGuiders.Platform.IntermediateRepresentation.Binding.csproj") '..\AIGuiders.Platform.IntermediateRepresentation.Keyboard\AIGuiders.Platform.IntermediateRepresentation.Keyboard.csproj'
Add-ProjectRef (Join-Path $irMel "AIGuiders.Platform.IntermediateRepresentation.Melody.csproj") '..\AIGuiders.Platform.IntermediateRepresentation.Argument\AIGuiders.Platform.IntermediateRepresentation.Argument.csproj'

# --- git mv: Argument ---
$argSrc = Join-Path $src 'AIGuiders.Platform.Notations.Argument'
foreach ($f in @('ArgumentNotationProfile.cs','NormalizedArguments.cs','ArgumentSlot.cs','ArgumentReaders.cs')) {
    git mv (Join-Path $argSrc $f) (Join-Path $irArg $f)
    Set-Namespace (Join-Path $irArg $f) 'AIGuiders.Platform.IntermediateRepresentation.Argument'
}

# --- git mv: Keyboard IR ---
git mv (Join-Path $src 'AIGuiders.Platform.Notations.Keyboard/NormalizedKeySequence.cs') (Join-Path $irKb 'NormalizedKeySequence.cs')
Set-Namespace (Join-Path $irKb 'NormalizedKeySequence.cs') 'AIGuiders.Platform.IntermediateRepresentation.Keyboard'

# --- git mv: Invocation ---
git mv (Join-Path $src 'AIGuiders.Platform.Notations.Command/NormalizedCommandLine.cs') (Join-Path $irInv 'NormalizedCommandLine.cs')
Set-Namespace (Join-Path $irInv 'NormalizedCommandLine.cs') 'AIGuiders.Platform.IntermediateRepresentation.Invocation'

# --- Bracket split ---
$brModels = Join-Path $src 'AIGuiders.Platform.Notations.Bracket/BracketNotationModels.cs'
$brIr = Join-Path $irBr 'BracketNotationModels.cs'
$models = Get-Content $brModels -Raw
$irPart = @'
#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Bracket;

public enum BracketAxisShape
{
    KeyValue = 0,
    Opaque = 1,
}

public sealed record BracketNotationProfile(
    string Id,
    string StartTerminal,
    string EndTerminal,
    char ListSeparator = ';',
    char KvSign = ':',
    BracketAxisShape AxisShape = BracketAxisShape.KeyValue,
    bool StripOuterTerminals = true,
    bool RespectBracketDepthOnListSplit = true,
    IReadOnlyList<string>? NestedAxisKeys = null);

public sealed record BracketAxis(
    string Key,
    char Sign,
    string Value,
    string ValueWireClass = BracketAxisValueClasses.Opaque,
    NormalizedBracketWire? Nested = null);

public static class BracketAxisValueClasses
{
    public const string Opaque = "opaque";
    public const string CommandPath = "command.path";
    public const string Kv = "notation.kv";
    public const string LineRange = "line.range";
    public const string NestedBracket = "bracket.nested";
}

public sealed record BracketAxisValuePlan(
    IReadOnlyDictionary<string, string> ByAxisKey,
    char DefaultValueKvSign = ':');

public sealed record NormalizedBracketWire(
    string ProfileId,
    IReadOnlyList<BracketAxis> Axes,
    string Raw);

public sealed record BracketAxisAliasMap(IReadOnlyDictionary<string, string> Aliases);

public static class BracketProfiles
{
    public static BracketNotationProfile CdpSquareKeyValue { get; } = new(
        "bracket.cdp-square-kv",
        "[",
        "]",
        NestedAxisKeys: ["Anchor"]);

    public static BracketNotationProfile SquareKeyValue => CdpSquareKeyValue;

    public static BracketNotationProfile AngleOpaque { get; } = new(
        "bracket.angle-opaque",
        "<",
        ">",
        AxisShape: BracketAxisShape.Opaque);

    public static BracketNotationProfile DocSymbol { get; } = new(
        "bracket.doc-symbol",
        "[",
        "]");
}

public static class BracketAxisValuePlans
{
    public static BracketAxisValuePlan CdpCode { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["F"] = BracketAxisValueClasses.CommandPath,
            ["File"] = BracketAxisValueClasses.CommandPath,
            ["M"] = BracketAxisValueClasses.Opaque,
            ["Member"] = BracketAxisValueClasses.Opaque,
            ["L"] = BracketAxisValueClasses.LineRange,
            ["Line"] = BracketAxisValueClasses.LineRange,
            ["S"] = BracketAxisValueClasses.Kv,
            ["Scope"] = BracketAxisValueClasses.Kv,
            ["K"] = BracketAxisValueClasses.Kv,
            ["Kind"] = BracketAxisValueClasses.Kv,
            ["T"] = BracketAxisValueClasses.Opaque,
            ["Text"] = BracketAxisValueClasses.Opaque,
            ["X"] = BracketAxisValueClasses.CommandPath,
            ["Element"] = BracketAxisValueClasses.CommandPath,
            ["A"] = BracketAxisValueClasses.Opaque,
            ["Attribute"] = BracketAxisValueClasses.Opaque,
            ["Anchor"] = BracketAxisValueClasses.NestedBracket,
            ["Command"] = BracketAxisValueClasses.Opaque,
            ["Go"] = BracketAxisValueClasses.Opaque,
            ["Family"] = BracketAxisValueClasses.Opaque,
        });

    public static BracketAxisValuePlan ForgeFrgCompound { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FRG"] = BracketAxisValueClasses.CommandPath,
        });

    public static BracketAxisValuePlan DocSymbol { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Family"] = BracketAxisValueClasses.Opaque,
            ["Package"] = BracketAxisValueClasses.Opaque,
            ["Type"] = BracketAxisValueClasses.Opaque,
            ["Member"] = BracketAxisValueClasses.Opaque,
            ["CatalogField"] = BracketAxisValueClasses.Opaque,
            ["Reader"] = BracketAxisValueClasses.Opaque,
        });
}
'@
$irPart | ForEach-Object { Write-Utf8 $brIr $_ }
$readerPart = @'
#nullable enable
using AIGuiders.Platform.IntermediateRepresentation.Bracket;
using AIGuiders.Platform.Notations;

namespace AIGuiders.Platform.Notations.Bracket;

public interface IBracketNotationReader
{
    bool TryRead(
        string wire,
        BracketNotationProfile profile,
        out NormalizedBracketWire? normalized,
        out string error);
}

public static class BracketAxisExtensions
{
    public static NotationKvPair ToKvPair(this BracketAxis axis) => new(axis.Key, axis.Sign, axis.Value);
}
'@
$readerPart | ForEach-Object { Write-Utf8 $brModels $_ }
git add $brIr $brModels

# --- git mv: Command catalog IR ---
$catDir = Join-Path $src 'AIGuiders.Platform.CommandPlane.Catalog/Catalog'
foreach ($f in @(
    'CommandDescriptor.cs','CatalogRouteEntry.cs','CatalogSemanticFields.cs','CatalogPathRole.cs',
    'CommandArgTailKind.cs','CommandArgTailPolicy.cs','CommandPickerChoiceKind.cs',
    'SlashConstructorBinding.cs','CommandDocumentFormat.cs')) {
    git mv (Join-Path $catDir $f) (Join-Path $irCmd $f)
    Set-Namespace (Join-Path $irCmd $f) 'AIGuiders.Platform.IntermediateRepresentation.Command'
}
Write-Utf8 (Join-Path $irCmd 'CommandDescriptor.cs') ((Get-Content (Join-Path $irCmd 'CommandDescriptor.cs') -Raw) -replace 'using AIGuiders\.Platform\.Notations\.Argument;', 'using AIGuiders.Platform.IntermediateRepresentation.Argument;')

# --- git mv: Binding IR ---
$bindDir = Join-Path $src 'AIGuiders.Platform.CommandPlane.Binding'
foreach ($f in @('BindingDescriptor.cs','BindingEntry.cs','BindingModels.cs')) {
    git mv (Join-Path $bindDir $f) (Join-Path $irBind $f)
    Set-Namespace (Join-Path $irBind $f) 'AIGuiders.Platform.IntermediateRepresentation.Binding'
}
Write-Utf8 (Join-Path $irBind 'BindingEntry.cs') ((Get-Content (Join-Path $irBind 'BindingEntry.cs') -Raw) -replace 'using AIGuiders\.Platform\.Notations\.Keyboard;', 'using AIGuiders.Platform.IntermediateRepresentation.Keyboard;')

# --- git mv: Melody IR ---
$melDir = Join-Path $src 'AIGuiders.Platform.CommandPlane.Melody'
foreach ($f in @('MelodyDescriptor.cs','MelodyStep.cs','MelodyLine.cs','MelodyLineProfile.cs','MelodyArticulation.cs')) {
    git mv (Join-Path $melDir $f) (Join-Path $irMel $f)
    Set-Namespace (Join-Path $irMel $f) 'AIGuiders.Platform.IntermediateRepresentation.Melody'
}
foreach ($f in @('MelodyDescriptor.cs','MelodyLine.cs')) {
    $p = Join-Path $irMel $f
    Write-Utf8 $p ((Get-Content $p -Raw) -replace 'using AIGuiders\.Platform\.Notations\.Argument;', 'using AIGuiders.Platform.IntermediateRepresentation.Argument;')
}

# --- Update Notations.Keyboard csproj + reader ---
Add-ProjectRef (Join-Path $src 'AIGuiders.Platform.Notations.Keyboard/AIGuiders.Platform.Notations.Keyboard.csproj') '..\AIGuiders.Platform.IntermediateRepresentation.Keyboard\AIGuiders.Platform.IntermediateRepresentation.Keyboard.csproj'
$ikb = Join-Path $src 'AIGuiders.Platform.Notations.Keyboard/IKeyboardNotationReader.cs'
Ensure-Using $ikb 'AIGuiders.Platform.IntermediateRepresentation.Keyboard'

# --- Notations.Command -> IR.Invocation ---
Add-ProjectRef (Join-Path $src 'AIGuiders.Platform.Notations.Command/AIGuiders.Platform.Notations.Command.csproj') '..\AIGuiders.Platform.IntermediateRepresentation.Invocation\AIGuiders.Platform.IntermediateRepresentation.Invocation.csproj'
$inv = Join-Path $src 'AIGuiders.Platform.Notations.Command/InvocationNotation.cs'
Ensure-Using $inv 'AIGuiders.Platform.IntermediateRepresentation.Invocation'

# --- Notations.Bracket -> IR.Bracket ---
Add-ProjectRef (Join-Path $src 'AIGuiders.Platform.Notations.Bracket/AIGuiders.Platform.Notations.Bracket.csproj') '..\AIGuiders.Platform.IntermediateRepresentation.Bracket\AIGuiders.Platform.IntermediateRepresentation.Bracket.csproj'
foreach ($f in @('BracketReader.cs','BracketEnvelopeScan.cs')) {
    $p = Join-Path $src "AIGuiders.Platform.Notations.Bracket/$f"
    Ensure-Using $p 'AIGuiders.Platform.IntermediateRepresentation.Bracket'
}

# --- Remove empty Notations.Argument project ---
git rm -r $argSrc

# --- Rewire csproj refs: Notations.Argument -> IR.Argument ---
Get-ChildItem -Recurse -Filter *.csproj $root | ForEach-Object {
    $t = Get-Content $_.FullName -Raw
    $new = $t -replace 'AIGuiders\.Platform\.Notations\.Argument\\AIGuiders\.Platform\.Notations\.Argument\.csproj', 'AIGuiders.Platform.IntermediateRepresentation.Argument\AIGuiders.Platform.IntermediateRepresentation.Argument.csproj'
    if ($new -ne $t) { Write-Utf8 $_.FullName $new }
}

# --- Guild package refs ---
$catalogCsproj = Join-Path $src 'AIGuiders.Platform.CommandPlane.Catalog/AIGuiders.Platform.CommandPlane.Catalog.csproj'
Replace-ProjectRef $catalogCsproj '..\AIGuiders.Platform.IntermediateRepresentation.Argument\AIGuiders.Platform.IntermediateRepresentation.Argument.csproj' '..\AIGuiders.Platform.IntermediateRepresentation.Command\AIGuiders.Platform.IntermediateRepresentation.Command.csproj'
Add-ProjectRef $catalogCsproj '..\AIGuiders.Platform.IntermediateRepresentation.Command\AIGuiders.Platform.IntermediateRepresentation.Command.csproj'

$bindingCsproj = Join-Path $src 'AIGuiders.Platform.CommandPlane.Binding/AIGuiders.Platform.CommandPlane.Binding.csproj'
Add-ProjectRef $bindingCsproj '..\AIGuiders.Platform.IntermediateRepresentation.Binding\AIGuiders.Platform.IntermediateRepresentation.Binding.csproj'

$melodyCsproj = Join-Path $src 'AIGuiders.Platform.CommandPlane.Melody/AIGuiders.Platform.CommandPlane.Melody.csproj'
Add-ProjectRef $melodyCsproj '..\AIGuiders.Platform.IntermediateRepresentation.Melody\AIGuiders.Platform.IntermediateRepresentation.Melody.csproj'

# --- slnx: add IR projects, remove Notations.Argument ---
$slnx = Join-Path $root 'AIGuiders.Platform.slnx'
$sl = Get-Content $slnx -Raw
$sl = $sl -replace '\s*<Project Path="src/AIGuiders\.Platform\.Notations\.Argument/AIGuiders\.Platform\.Notations\.Argument\.csproj" />\r?\n', ''
$insert = @'
    <Project Path="src/AIGuiders.Platform.IntermediateRepresentation.Argument/AIGuiders.Platform.IntermediateRepresentation.Argument.csproj" />
    <Project Path="src/AIGuiders.Platform.IntermediateRepresentation.Binding/AIGuiders.Platform.IntermediateRepresentation.Binding.csproj" />
    <Project Path="src/AIGuiders.Platform.IntermediateRepresentation.Bracket/AIGuiders.Platform.IntermediateRepresentation.Bracket.csproj" />
    <Project Path="src/AIGuiders.Platform.IntermediateRepresentation.Command/AIGuiders.Platform.IntermediateRepresentation.Command.csproj" />
    <Project Path="src/AIGuiders.Platform.IntermediateRepresentation.Invocation/AIGuiders.Platform.IntermediateRepresentation.Invocation.csproj" />
    <Project Path="src/AIGuiders.Platform.IntermediateRepresentation.Keyboard/AIGuiders.Platform.IntermediateRepresentation.Keyboard.csproj" />
    <Project Path="src/AIGuiders.Platform.IntermediateRepresentation.Melody/AIGuiders.Platform.IntermediateRepresentation.Melody.csproj" />
'@
$sl = $sl -replace '(<Project Path="src/AIGuiders\.Platform\.Catalog/AIGuiders\.Platform\.Catalog\.csproj" />)', "$insert`n    `$1"
Write-Utf8 $slnx $sl

# --- Bulk using rewrites in .cs ---
$csFiles = Get-ChildItem -Recurse -Filter *.cs $root | Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' }
foreach ($file in $csFiles) {
    $c = Get-Content $file.FullName -Raw
    $orig = $c
    $c = $c -replace 'using AIGuiders\.Platform\.Notations\.Argument;', 'using AIGuiders.Platform.IntermediateRepresentation.Argument;'
    $c = $c -replace 'using AIGuiders\.Platform\.Notations\.Bracket;', 'using AIGuiders.Platform.IntermediateRepresentation.Bracket;'
    if ($c -match 'NormalizedKeySequence|NormalizedSequenceStep|NormalizedChordStep|NormalizedPlainKeyStep|ChordModifierKeys') {
        $c = $c -replace 'using AIGuiders\.Platform\.Notations\.Keyboard;', "using AIGuiders.Platform.IntermediateRepresentation.Keyboard;`nusing AIGuiders.Platform.Notations.Keyboard;"
        if ($c -notmatch 'IntermediateRepresentation\.Keyboard') {
            $c = "using AIGuiders.Platform.IntermediateRepresentation.Keyboard;`n$c"
        }
    }
    if ($c -match 'NormalizedCommandLine') {
        if ($c -notmatch 'IntermediateRepresentation\.Invocation') {
            if ($c -match '(?m)^using ') { $c = $c -replace '(?m)(^using [^;]+;\r?\n)', "`$1using AIGuiders.Platform.IntermediateRepresentation.Invocation;`n", 1 }
            else { $c = "using AIGuiders.Platform.IntermediateRepresentation.Invocation;`n$c" }
        }
    }
    $cmdTypes = 'CommandDescriptor|CommandPickerChoice|CatalogRouteEntry|CatalogSemanticFields|CatalogPathRole|CommandArgTailKind|CommandArgTailPolicy|CommandPickerChoiceKind|SlashConstructorBinding|CommandDocumentFormat'
    if ($c -match $cmdTypes) {
        if ($c -notmatch 'IntermediateRepresentation\.Command') {
            if ($c -match '(?m)^using ') { $c = $c -replace '(?m)(^using [^;]+;\r?\n)', "`$1using AIGuiders.Platform.IntermediateRepresentation.Command;`n", 1 }
            else { $c = "using AIGuiders.Platform.IntermediateRepresentation.Command;`n$c" }
        }
    }
    $bindTypes = 'BindingDescriptor|BindingEntry|BindingTargetKind|BindingDocumentFormat'
    if ($c -match $bindTypes) {
        if ($c -notmatch 'IntermediateRepresentation\.Binding') {
            if ($c -match '(?m)^using ') { $c = $c -replace '(?m)(^using [^;]+;\r?\n)', "`$1using AIGuiders.Platform.IntermediateRepresentation.Binding;`n", 1 }
            else { $c = "using AIGuiders.Platform.IntermediateRepresentation.Binding;`n$c" }
        }
    }
    $melTypes = 'MelodyDescriptor|MelodyStep|MelodyLine|MelodyLineProfile|MelodyArticulation'
    if ($c -match $melTypes) {
        if ($c -notmatch 'IntermediateRepresentation\.Melody') {
            if ($c -match '(?m)^using ') { $c = $c -replace '(?m)(^using [^;]+;\r?\n)', "`$1using AIGuiders.Platform.IntermediateRepresentation.Melody;`n", 1 }
            else { $c = "using AIGuiders.Platform.IntermediateRepresentation.Melody;`n$c" }
        }
    }
    if ($c -ne $orig) { Write-Utf8 $file.FullName $c }
}

# --- GlobalUsings ---
$gu = Join-Path $src 'AIGuiders.Platform.CommandPlane.ArgSuggestions/GlobalUsings.cs'
Write-Utf8 $gu "global using AIGuiders.Platform.IntermediateRepresentation.Command;`n"

Write-Host 'IR migration script completed.'
