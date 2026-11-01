$ErrorActionPreference = 'Stop'
$base = $PSScriptRoot
$documentPath = Join-Path $base 'CONVENTIONS.md'
$progressPath = Join-Path $base 'DOCUMENTING.progress.json'
$manifestPath = Join-Path $base 'consistency.json'
$todo = Get-Content -LiteralPath (Join-Path $base 'TODO.md') -Raw
$progress = Get-Content -LiteralPath $progressPath -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$pairs = [ordered]@{'CONS-009'='C12';'CONS-010'='C13';'CONS-011'='C46';'CONS-012'='C16';'CONS-013'='C06';'CONS-014'='C36';'CONS-015'='C22';'CONS-016'='C37';'CONS-017'='C45'}
if ($progress.policy_adoption.authorization -notmatch 'approved all nine') { throw 'Missing explicit approval record' }
function Save-Progress {
    $progress.updated_at = [DateTimeOffset]::Now.ToString('o')
    $progress | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath "$progressPath.tmp" -Encoding utf8
    Move-Item -LiteralPath "$progressPath.tmp" -Destination $progressPath -Force
}
foreach ($task in $pairs.Keys) {
    $id = $pairs[$task]
    $unit = $progress.work_units | Where-Object id -eq $id
    if ($unit.adoption_state -eq 'complete') { continue }
    $proposal = [regex]::Match($todo, '(?ms)^- \[ \] \*\*' + $task + ' .*?(?=^<a id="cons-|^## P1)')
    $target = [regex]::Match($proposal.Value, '(?ms)^  \*\*Recommended target:\*\* (.*?)(?=^  \*\*Alternative)').Groups[1].Value.Trim()
    $acceptance = [regex]::Match($proposal.Value, '(?ms)^  \*\*Dependencies / risk / acceptance:\*\* (.*)\z').Groups[1].Value.Trim()
    if (!$target -or !$acceptance) { throw "Missing proposal fields for $task" }
    $unit.state = 'in_progress'
    $progress.next_step = "Adopt and verify $id from approved $task"
    Save-Progress
    $text = Get-Content -LiteralPath $documentPath -Raw
    $pattern = '(?ms)^## ' + $id + ' .*?(?=^<a id="c|^## Completion|\z)'
    $section = [regex]::Match($text, $pattern)
    if (!$section.Success) { throw "Missing criterion $id" }
    $selected = "**Selected target:** **Existing rule** (explicit user adoption of $task on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. $target`n`n"
    $record = [regex]::Replace($section.Value, '(?ms)^\*\*Selected target:\*\*.*?(?=^\*\*Inspection and compliance / open decisions:\*\*)', [System.Text.RegularExpressions.MatchEvaluator]{ param($match) $selected })
    $check = "**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. $acceptance`n`n**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#$($task.ToLowerInvariant())). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.`n`n"
    $record = [regex]::Replace($record, '(?ms)^\*\*Inspection and compliance / open decisions:\*\*.*\z', [System.Text.RegularExpressions.MatchEvaluator]{ param($match) $check })
    $text = $text.Substring(0, $section.Index) + $record + $text.Substring($section.Index + $section.Length)
    if ($task -eq 'CONS-009') {
        $notice = "Policy update on 2026-10-30: the user adopted the complete recommendations CONS-009 through CONS-017. Their targets are normative for the continuing full audit. Existing implementations are assessed separately; the original discovery baseline and observations remain historical.`n`n"
        $text = $text.Replace('## Basis and limits', $notice + '## Basis and limits')
    }
    if ($task -eq 'CONS-017') {
        $summary = @'
## Completion and approved policies

All 50 criteria have selected auditable targets: 32 Existing rule and 18 Established pattern records, with no N/A criteria or unresolved policy decisions. On 2026-10-30 the user explicitly adopted CONS-009 through CONS-017, resolving the six previous Decision required records and adopting the three previous Proposed target records. The exact approved recommendations and their alternatives remain in the single TODO as decision history.

The approved policies address cancellation/admission (C12), resource ownership and cleanup (C13), host concurrency/shutdown (C46), diagnostics (C16), namespace mapping (C06), public XML documentation (C36), helper ownership (C22), implementation comments/suppressions (C37) and qualitative responsibility review (C45). They are targets for audit, not claims that their future remediation is already complete. Existing public API, wire/persisted compatibility and the named CONS-007 failure policy remain constraints.

Original sampled discovery, its baseline, observations and fingerprints remain historical. The 2026-10-30 policy input snapshot enumerates 783 library and 258 test C# files and records namespace/configuration discovery; it does not certify every source file against every criterion. Existing CONS-001 through CONS-008 repairs were completed separately and must be revalidated against current inputs during audit.

The authorized next action is the full solution consistency audit, including C47 revalidation. It must pin this convention revision/digest, enumerate complete eligible sets by criterion/role, assess each unit and map every deviation to the existing common TODO. No production source change is performed by documentation or audit.
'@
        $text = [regex]::Replace($text, '(?ms)^## Completion and unresolved decisions.*\z', [System.Text.RegularExpressions.MatchEvaluator]{ param($match) $summary })
    }
    [IO.File]::WriteAllText($documentPath, $text, [Text.UTF8Encoding]::new($false))
    if ([regex]::Matches($text, '(?m)^## C\d{2} ').Count -ne 50) { throw 'Criterion reconciliation failed' }
    $hash = (Get-FileHash -LiteralPath $documentPath).Hash
    $manifest.conventions_revision++
    $manifest.conventions_digest = $hash
    $manifest | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $manifestPath -Encoding utf8
    $unit.state = 'complete'
    $unit.adoption_state = 'complete'
    $unit.remaining = @()
    $unit.target_label = 'Existing rule'
    $unit | Add-Member -Force NoteProperty adopted_policy $task
    $unit | Add-Member -Force NoteProperty policy_evidence @('TODO.md#'+$task.ToLowerInvariant(), 'AUDIT.policy-inputs.json')
    $progress.checkpoint++
    $progress.outputs.conventions.sha256 = $hash
    $progress.outputs.conventions.checkpoint = $progress.checkpoint
    $progress.next_step = 'Adopt next approved criterion; after all nine, validate and resume the full audit.'
    Save-Progress
}
$progress.status = 'complete'
$progress.decisions = @()
$progress.blockers = @()
$progress.policy_adoption.status = 'complete'
$progress.policy_adoption | Add-Member -Force NoteProperty completed_revision $manifest.conventions_revision
$progress.policy_adoption | Add-Member -Force NoteProperty completed_digest $manifest.conventions_digest
$progress.next_step = 'All nine approved policies documented. Continue the authorized full audit pinned to the new convention revision/digest.'
$progress.validation += [pscustomobject]@{check='Explicit policy adoption';result='pass';criteria=@($pairs.Values);revision=$manifest.conventions_revision;digest=$manifest.conventions_digest;remaining_policy_decisions=0}
Save-Progress
[pscustomobject]@{documenting_status=$progress.status;adopted=$pairs.Count;revision=$manifest.conventions_revision;digest=$manifest.conventions_digest} | ConvertTo-Json
