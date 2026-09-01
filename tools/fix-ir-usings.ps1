$root = Split-Path $PSScriptRoot -Parent
$patterns = @(
  @{ Old = 'using AIGuiders.Platform.Notations.Argument;'; New = 'using AIGuiders.Platform.IntermediateRepresentation.Argument;' }
  @{ Old = 'using AIGuiders.Platform.Notations.Bracket;'; New = 'using AIGuiders.Platform.IntermediateRepresentation.Bracket;' }
)
function Test-HasGlobalIrUsings($file) {
  $dir = Split-Path $file -Parent
  while ($dir -and (Test-Path $dir)) {
    $global = Get-ChildItem -Path $dir -Filter 'GlobalUsings*.cs' -File -ErrorAction SilentlyContinue
    foreach ($g in $global) {
      $text = [IO.File]::ReadAllText($g.FullName)
      if ($text -match 'global using AIGuiders\.Platform\.IntermediateRepresentation\.') { return $true }
    }
    $parent = Split-Path $dir -Parent
    if ($parent -eq $dir) { break }
    $dir = $parent
  }
  return $false
}
Get-ChildItem -Path $root -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' } | ForEach-Object {
  if (Test-HasGlobalIrUsings $_.FullName) { return }
  $c = [IO.File]::ReadAllText($_.FullName)
  $orig = $c
  foreach ($p in $patterns) { $c = $c.Replace($p.Old, $p.New) }
  if ($c -match 'NormalizedKeySequence|NormalizedSequenceStep|NormalizedChordStep|NormalizedPlainKeyStep|ChordModifierKeys') {
    if ($c -notmatch 'IntermediateRepresentation\.Keyboard') {
      $c = "using AIGuiders.Platform.IntermediateRepresentation.Keyboard;`r`n" + $c
    }
  }
  if ($c -match 'NormalizedCommandLine' -and $c -notmatch 'IntermediateRepresentation\.Invocation') {
    $c = "using AIGuiders.Platform.IntermediateRepresentation.Invocation;`r`n" + $c
  }
  if ($c -match 'CommandDescriptor|CommandPickerChoice|CatalogRouteEntry|CatalogSemanticFields|CatalogPathRole|CommandArgTailKind|CommandArgTailPolicy|CommandPickerChoiceKind|SlashConstructorBinding|CommandDocumentFormat') {
    if ($c -notmatch 'IntermediateRepresentation\.Command' -and $_.FullName -notmatch 'IntermediateRepresentation\.Command') {
      $c = "using AIGuiders.Platform.IntermediateRepresentation.Command;`r`n" + $c
    }
  }
  if ($c -match 'BindingDescriptor|BindingEntry|BindingTargetKind|BindingDocumentFormat') {
    if ($c -notmatch 'IntermediateRepresentation\.Binding' -and $_.FullName -notmatch 'IntermediateRepresentation\.Binding') {
      $c = "using AIGuiders.Platform.IntermediateRepresentation.Binding;`r`n" + $c
    }
  }
  if ($c -match 'MelodyDescriptor|MelodyStep|MelodyLine|MelodyLineProfile|MelodyArticulation') {
    if ($c -notmatch 'IntermediateRepresentation\.Melody' -and $_.FullName -notmatch 'IntermediateRepresentation\.Melody') {
      $c = "using AIGuiders.Platform.IntermediateRepresentation.Melody;`r`n" + $c
    }
  }
  if ($c -ne $orig) { [IO.File]::WriteAllText($_.FullName, $c) }
}
