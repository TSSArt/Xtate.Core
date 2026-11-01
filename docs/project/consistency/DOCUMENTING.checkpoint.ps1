param([string]$RecordsPath)
$ErrorActionPreference = 'Stop'
$artifactDir = $PSScriptRoot
$sourceRoot = [IO.Path]::GetFullPath((Join-Path $artifactDir '../../..'))
$progressPath = Join-Path $artifactDir 'DOCUMENTING.progress.json'
$manifestPath = Join-Path $artifactDir 'consistency.json'
$documentPath = Join-Path $artifactDir 'CONVENTIONS.md'
$progress = Get-Content -LiteralPath $progressPath -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$evidence = Get-Content (Join-Path $artifactDir 'EVIDENCE.json') -Raw | ConvertFrom-Json
$inventory = Get-Content (Join-Path $artifactDir 'INVENTORY.json') -Raw | ConvertFrom-Json
function Save-Progress {
    $progress.updated_at = [DateTimeOffset]::UtcNow.ToString('o')
    $progress | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath "$progressPath.tmp"
    Move-Item -LiteralPath "$progressPath.tmp" -Destination $progressPath -Force
}
foreach ($row in (Get-Content -LiteralPath $RecordsPath | Where-Object { $_ -and !$_.StartsWith('#') })) {
    $parts = $row.Split('|')
    if ($parts.Count -ne 9) { throw "Expected 9 fields: $row" }
    $id,$name,$scope,$label,$confidence,$refs,$observed,$rule,$check = $parts
    $keys = $refs.Split(',')
    $paths = @($keys | ForEach-Object {
        $path = $evidence.$_
        if (!$path -or !(Test-Path -LiteralPath (Join-Path $sourceRoot $path))) { throw "Missing evidence: $_" }
        $path
    } | Select-Object -Unique)
    $unit = $progress.work_units | Where-Object id -EQ $id
    if ($unit.state -eq 'complete') { throw "Unit already complete: $id; reconcile before rewriting" }
    $unit.state = 'in_progress'
    $unit.eligible_set = $paths
    $unit.remaining = $paths
    $progress.next_step = "Save and verify $id ($name); sampled file cohort recorded in eligible_set."
    Save-Progress
    $links = @($keys | ForEach-Object { "[$_]`(../../../$($evidence.$_)`)" }) -join ', '
    $sourceCount = @($paths | Where-Object { $_.EndsWith('.cs') }).Count
    $text = @"

<a id="$($id.ToLower())"></a>

## $id — $name

**Applicability and comparison unit:** Applicable. $scope

**Evidence and discovery coverage:** $($paths.Count)/$($paths.Count) selected evidence files inspected for this criterion ($sourceCount C# files); $links. This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** $observed

**Selected target:** **$label**. Confidence: **$confidence**. $rule

**Inspection and compliance / open decisions:** $check

"@
    Add-Content -LiteralPath $documentPath -Value $text
    $saved = Get-Content -LiteralPath $documentPath -Raw
    if ([regex]::Matches($saved, "(?m)^## $id — ").Count -ne 1) { throw "Duplicate or missing $id" }
    $unit.state = 'complete'
    $unit.processed = $paths
    $unit.remaining = @()
    $unit | Add-Member -NotePropertyName output -NotePropertyValue "CONVENTIONS.md#$($id.ToLower())" -Force
    $unit | Add-Member -NotePropertyName target_label -NotePropertyValue $label -Force
    $unit | Add-Member -NotePropertyName discovery_method -NotePropertyValue 'Targeted file/control-flow inspection; evidence file sample, not declaration-frequency census' -Force
    $progress.checkpoint++
    $hash = (Get-FileHash -LiteralPath $documentPath -Algorithm SHA256).Hash
    $progress.outputs = @{conventions=@{path='CONVENTIONS.md';sha256=$hash;checkpoint=$progress.checkpoint};inventory=@{path='INVENTORY.json';sha256=(Get-FileHash (Join-Path $artifactDir 'INVENTORY.json')).Hash}}
    if ($label -eq 'Decision required') {
        $progress.decisions += @{criterion=$id;scope=$scope;question=$check;auditable_target=$false}
    }
    $manifest.conventions_revision++
    $manifest.conventions_digest = $hash
    $manifest | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $manifestPath
    $progress.next_step = 'Process next pending criterion; after all 50, validate links, hashes, scope reconciliation and source stability.'
    Save-Progress
}
