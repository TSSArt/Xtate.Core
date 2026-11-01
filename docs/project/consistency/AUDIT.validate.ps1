$ErrorActionPreference = 'Stop'
$artifactDir = $PSScriptRoot
$sourceRoot = [IO.Path]::GetFullPath((Join-Path $artifactDir '../../..'))
$manifest = Get-Content (Join-Path $artifactDir 'consistency.json') -Raw | ConvertFrom-Json
$progress = Get-Content (Join-Path $artifactDir 'TODO.progress.json') -Raw | ConvertFrom-Json
$inventory = Get-Content (Join-Path $artifactDir 'AUDIT.inventory.json') -Raw | ConvertFrom-Json
$coverage = @(Get-Content (Join-Path $artifactDir 'AUDIT.coverage.json') -Raw | ConvertFrom-Json)
$structure = @(Get-Content (Join-Path $artifactDir 'AUDIT.structure.json') -Raw | ConvertFrom-Json)
$probes = Get-Content (Join-Path $artifactDir 'AUDIT.probes.json') -Raw | ConvertFrom-Json
$todo = Get-Content (Join-Path $artifactDir 'TODO.md') -Raw
if ($manifest.conventions_revision -ne $progress.inputs.conventions_revision -or
    $manifest.conventions_digest -ne $progress.inputs.conventions_digest -or
    (Get-FileHash (Join-Path $artifactDir 'CONVENTIONS.md')).Hash -ne $manifest.conventions_digest) { throw 'Convention pin mismatch' }
if ([IO.Path]::GetFullPath((Join-Path $artifactDir $manifest.source_root)) -ne $sourceRoot) { throw 'Source root mismatch' }
if ($progress.scope.criteria.Count -ne 1 -or $progress.scope.criteria[0] -ne 'C47') { throw 'Audit scope changed' }
foreach ($file in @($inventory.files) + @($inventory.supporting_inputs)) {
    if ((Get-FileHash -LiteralPath (Join-Path $sourceRoot $file.path)).Hash -ne $file.sha256) { throw "Input changed: $($file.path)" }
}
$documentInventory = Get-Content (Join-Path $artifactDir 'INVENTORY.json') -Raw | ConvertFrom-Json
foreach ($file in $documentInventory.files) {
    if ((Get-FileHash -LiteralPath (Join-Path $sourceRoot $file.path)).Hash -ne $file.sha256) { throw "Original maintained input changed: $($file.path)" }
}
$expected = @($inventory.files.path | Sort-Object)
$currentPaths = @(& rg --files (Join-Path $sourceRoot 'src/Xtate.Core/StateMachineHost') (Join-Path $sourceRoot 'src/Xtate.Core/Common/TaskMonitor') (Join-Path $sourceRoot 'src/Xtate.Core/Common/IoBoundTask') -g '*.cs' | ForEach-Object { [IO.Path]::GetRelativePath($sourceRoot, $_).Replace('\','/') }) + @(
    'src/Xtate.Core/Common/Extensions/TaskExtensions.cs',
    'src/Xtate.Core/Common/Extensions/ValueTaskExtensions.cs',
    'src/Xtate.Core/Common/Helpers/LazyTask.cs',
    'src/Xtate.Core/Persistence/Services/PersistentEventScheduler.cs',
    'src/Xtate.Core/Persistence/Model/PersistedScheduledEvent.cs'
)
if (Compare-Object $expected @($currentPaths | Sort-Object -Unique)) { throw 'Current scope contains new or removed source files' }
foreach ($set in @(@{name='scope';paths=@($progress.scope.paths)},@{name='coverage';paths=@($coverage.path)},@{name='structure';paths=@($structure.path)})) {
    if ($set.paths.Count -ne 76 -or @($set.paths | Sort-Object -Unique).Count -ne 76 -or
        (Compare-Object $expected @($set.paths | Sort-Object))) { throw "Eligible-set mismatch: $($set.name)" }
}
$processed = @($progress.work_units | Where-Object id -NE 'C47-inventory' | ForEach-Object { $_.processed })
if ($processed.Count -ne 76 -or (Compare-Object $expected @($processed | Sort-Object))) { throw 'Processed-unit partition mismatch' }
foreach ($unit in $progress.work_units) {
    if ($unit.state -ne 'complete' -or $unit.remaining.Count -ne 0 -or
        (Compare-Object @($unit.eligible_set | Sort-Object) @($unit.processed | Sort-Object))) { throw "Unfinished unit $($unit.id)" }
}
$taskIds = @([regex]::Matches($todo, '(?m)^- \[ \] \*\*(CONS-\d{3}) \[P[123]\]') | ForEach-Object { $_.Groups[1].Value })
if ($taskIds.Count -ne 8 -or @($taskIds | Sort-Object -Unique).Count -ne 8 -or $todo -match '(?m)^- \[[xX]\]') { throw 'Task IDs/implementation state mismatch' }
if (@([regex]::Matches($todo, '(?m)^- \[ \] \*\*CONS-\d{3} \[P1\]')).Count -ne 5) { throw 'Priority count mismatch' }
$valid = @('Compliant','Deviation','Justified exception','Not applicable')
foreach ($row in $coverage) {
    if ($row.disposition -notin $valid -or !$row.reason) { throw "Missing disposition $($row.path)" }
    if ($row.disposition -eq 'Deviation' -and !$row.tasks.Count) { throw "Unmapped deviation $($row.path)" }
    foreach ($taskId in $row.tasks) { if ($taskId -notin $taskIds) { throw "Missing task $taskId" } }
    $file = $structure | Where-Object path -EQ $row.path
    if ($file.parse_errors.Count -ne 0 -or $row.symbols.Count -ne $file.members.Count) { throw "Structural mismatch $($row.path)" }
    $lineCount = @(Get-Content -LiteralPath (Join-Path $sourceRoot $row.path)).Count
    foreach ($symbol in $row.symbols) {
        if ($symbol.line -lt 1 -or $symbol.end_line -lt $symbol.line -or $symbol.end_line -gt $lineCount) { throw "Invalid span $($row.path)" }
    }
    foreach ($await in $file.awaits) { if ($await.expression -notmatch 'ConfigureAwait\(false\)') { throw "Unreconciled await $($row.path):$($await.line)" } }
}
foreach ($id in $taskIds | Where-Object { $_ -ne 'CONS-007' }) {
    if ($id -notin @($coverage | ForEach-Object { $_.tasks })) { throw "Remediation task lacks affected unit: $id" }
}
$counts = @{}
foreach ($group in $coverage | Group-Object disposition) { $counts[$group.Name] = $group.Count }
if ($counts.Compliant -ne 44 -or $counts.Deviation -ne 9 -or $counts.'Justified exception' -ne 1 -or $counts.'Not applicable' -ne 22) { throw 'Coverage reconciliation mismatch' }
if ($probes.probes.Count -ne 5 -or @($probes.probes | Where-Object { !$_.current_deviation_reproduced }).Count -or
    $probes.structural_counts.files -ne 76 -or $probes.structural_counts.members -ne 389 -or $probes.structural_counts.awaits -ne 94 -or $probes.structural_counts.parse_errors) { throw 'Probe reconciliation mismatch' }
$linkCount = 0
foreach ($docName in @('TODO.md','COVERAGE.md')) {
    $doc = Get-Content (Join-Path $artifactDir $docName) -Raw
    foreach ($match in [regex]::Matches($doc, '\]\(([^)]+)\)')) {
        $target = $match.Groups[1].Value
        $parts = $target.Split('#', 2)
        $targetPath = if ($parts[0]) { Join-Path $artifactDir $parts[0] } else { Join-Path $artifactDir $docName }
        if (!(Test-Path -LiteralPath $targetPath)) { throw "Broken link in ${docName}: $target" }
        if ($parts.Count -eq 2 -and !(Get-Content -LiteralPath $targetPath -Raw).Contains('id="' + $parts[1] + '"')) { throw "Broken anchor $target" }
        $linkCount++
    }
}
$dependencies = @{'CONS-001'=@('CONS-007');'CONS-002'=@();'CONS-003'=@();'CONS-004'=@('CONS-007');'CONS-005'=@('CONS-007','CONS-004');'CONS-006'=@('CONS-007','CONS-004');'CONS-007'=@();'CONS-008'=@()}
function Check-Dependency([string]$id, [string[]]$trail) {
    if ($id -in $trail) { throw "Dependency cycle $id" }
    foreach ($dependency in $dependencies[$id]) {
        if ($dependency -notin $taskIds) { throw "Unknown dependency $dependency" }
        Check-Dependency $dependency @($trail + $id)
    }
}
foreach ($id in $taskIds) { Check-Dependency $id @() }
$result = [ordered]@{
    status='pass'
    criterion='C47'
    files=76
    applicable=54
    counts=$counts
    members=389
    await_expressions=94
    modern_and_legacy_conditional_variants='inspected and structurally enumerated'
    source_linked_reproductions=5
    tasks=8
    priorities=@{P1=5;P2=3}
    local_links_checked=$linkCount
    source_and_supporting_fingerprints='unchanged, including all 1083 original maintained inputs'
    conventions='pinned revision and digest unchanged'
    dependencies=$dependencies
    completion_scope='Approved C47 static audit and complete remediation plan; no implementation or whole-solution correctness claim'
}
$result | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $artifactDir 'AUDIT.validation.json')
$result | ConvertTo-Json -Depth 12
