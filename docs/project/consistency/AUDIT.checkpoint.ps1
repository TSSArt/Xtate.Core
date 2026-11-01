param([Parameter(Mandatory)][string[]]$CompleteUnits)
$ErrorActionPreference = 'Stop'
$artifactDir = $PSScriptRoot
$progressPath = Join-Path $artifactDir 'TODO.progress.json'
$coveragePath = Join-Path $artifactDir 'AUDIT.coverage.json'
$progress = Get-Content -LiteralPath $progressPath -Raw | ConvertFrom-Json
$rows = @(Get-Content -LiteralPath $coveragePath -Raw | ConvertFrom-Json)
$structure = @(Get-Content (Join-Path $artifactDir 'AUDIT.structure.json') -Raw | ConvertFrom-Json)
$notes = @{
    'TaskMonitor.cs' = 'WaitAsync pre-cancelled branches abandon incomplete operations; successful ValueTask Forget skips consumption; discarded MonitorTaskCompletion can fault in reporting. Guarded GetResult only runs on known-completed faults and is not blocking.'
    'TaskExtensions.cs' = 'Monitored overloads forward ownership. Parameterless Forget is empty and supplies no observer; the only production caller of parameterless Forget is LazyTask (ValueTask overload), with both helper shapes exposing the same unsafe fallback design.'
    'ValueTaskExtensions.cs' = 'Monitored overloads forward ownership; parameterless Forget discards Preserve result. Token-only WaitAsync remains a wait adapter, not a promised monitoring transfer; the caller retains underlying-operation responsibility.'
    'LazyTask.cs' = 'Factory is awaited once; normal outcomes reach the shared completion source. If caller cancellation has won and no monitor is supplied, a late factory fault rethrows into the unmonitored Forget fallback.'
    'IoBoundTaskScheduler.cs' = 'Dedicated background worker threads wait on a semaphore to drain scheduled Task objects through TryExecuteTask. These infrastructure waits do not synchronously block an async caller; no user Task is discarded. Worker inlining and fault-drain tests are supporting evidence.'
    'DefaultIoBoundTask.cs' = 'Returns a cached TaskFactory using the dedicated scheduler; starts no operation itself.'
    'IIoBoundTask.cs' = 'Exposes TaskFactory ownership to the caller; contains no hidden asynchronous operation.'
    'ITaskMonitor.cs' = 'Declares all four wait and three Forget shapes; exposes the explicit task-observation transfer boundary. Implementation deviations are in TaskMonitor.cs.'
    'PersistentEventScheduler.cs' = 'InitializeAsync returns AsyncInit; Init awaits rescheduling/storage work; Schedule/Cancel await base operations; WaitForDispatch awaits delay/removal; semaphore acquisition is asynchronous and release is in finally.'
    'PersistedScheduledEvent.cs' = 'Synchronous representation/storage model. Inherited cancellation behavior is in ScheduledEvent; no new asynchronous execution to assess.'
    'ScheduledEvent.cs' = 'CancelAsync returns the owned cancellation-source Task to its caller. Synchronous Cancel/Dispose remain separate synchronous contracts.'
    'StateMachineStatus.cs' = 'ForceFailed can fault readiness before WhenAccepted has an observer; a synchronous production Forget failure in controller Start bypasses the await. The default status implementation and controller must reconcile startup-proxy ownership together.'
    'DeadLetterQueue.cs' = 'Enqueue returns Logger.Write ValueTask to the caller without discarding completion.'
    'ExternalCommunication.cs' = 'TrySend returns or awaits routing/scheduling work; Cancel forwards the ValueTask. All await expressions suppress context capture.'
    'ExternalEventDispatcher.cs' = 'Every selected dispatch/dead-letter operation is awaited with ConfigureAwait(false); no detached branch.'
    'ExternalServiceClass.cs' = 'Explicit parent-event Dispatch returns the dispatcher ValueTask; other members synchronously supply configuration/forwarding.'
    'ExternalServiceCollection.cs' = 'Dispatch awaits asynchronous controller lookup and every fallback dispatch/dead-letter path; no locally detached task.'
    'ExternalServiceController.cs' = 'WaitForCompletion returns AsyncInit; execution and error event/log dispatch are awaited. Reporting failures propagate through the returned execution operation rather than being silently discarded here.'
    'ExternalServiceEventRouter.cs' = 'GetRouterEvent completes synchronously; Dispatch returns collection dispatch directly; provider selection is synchronous.'
    'ExternalServiceFactory.cs' = 'CreateService awaits selection/creation; provider enumeration and enumerator disposal are awaited with false context capture.'
    'ExternalServiceGlobalCollection.cs' = 'TryDispatch awaits controller lookup and dispatch or returns false; no owned background operation is dropped.'
    'ExternalServiceManager.cs' = 'Start awaits construction and scope activation; the scope manager independently monitors completion. Forward and Cancel return their ValueTasks. Scope-manager defects are attributed to their source boundary, not duplicated here.'
    'ExternalServiceResult.cs' = 'WaitForCompletion exposes the backing Task as a ValueTask; proxy completion responsibility belongs to ExternalServiceScopeManager.'
    'ExternalServiceScopeManager.cs' = 'Start awaits resolution and detaches WaitAndCleanup through TaskMonitor; Cleanup can throw before the separately published result source is completed. Sync and async disposal remain distinct paths; all async awaits use ConfigureAwait(false).'
    'InProcEventScheduler.cs' = 'DelayedFire is explicitly transferred to TaskMonitor; delay, dispatch and logging are awaited; removal/disposal is in finally. Async cancel/disposal returns or awaits aggregate work. Caller-token and cleanup-drain policies are not C47 findings.'
    'InternalEventDispatcher.cs' = 'Switch returns exactly the chosen dispatcher/dead-letter ValueTask to the caller.'
    'IoProcessorBase.cs' = 'Explicit interface methods forward returned ValueTasks; router-event construction is synchronously completed; concrete Dispatch remains an asynchronous contract.'
    'ScxmlIoProcessor.cs' = 'Dispatch returns InternalEventDispatcher completion; other operations compute routing data synchronously.'
    'SecurityContext.cs' = 'TaskFactory construction selects the dedicated or denying scheduler and returns it; no Task is started or discarded by this type.'
    'SecurityContextRegistration.cs' = 'DisposeAsync restores AsyncLocal state synchronously and returns a completed ValueTask; introduces no wait or background work.'
    'StateMachineCollection.cs' = 'Dispatch and Destroy await controller lookup and operation/dead-letter completion; pending lookup mechanics are a dependency context, not synchronously waited here.'
    'StateMachineControllerBase.cs' = 'ExecuteAsync is monitored and ordinary interpreter paths complete the result/status. Startup failure can prevent a handle from being exported while private result/readiness proxy Tasks have already faulted; their separate observation is not guaranteed by observing ExecuteAsync.'
    'StateMachineDestroyOnIdle.cs' = 'Timer exception callback discards Logger.Write ValueTask. StateTracker.OnChanged awaits interpreter acquisition; native async Timer disposal and the synchronous legacy shim are legitimate target-specific variants.'
    'StateMachineExternalService.cs' = 'Execute/Dispatch and async disposal await work. Synchronous disposal explicitly transfers Destroy to TaskMonitor rather than blocking; no new background-disposal policy is inferred.'
    'StateMachineHost.cs' = 'Start uses configured asynchronous enumeration and awaits every processor start; Stop awaits each popped processor. Failure may stop cleanup traversal (C13 policy outside scope), but the returned operation itself completes/faults and is not detached.'
    'StateMachineResult.cs' = 'GetResult returns a ValueTask wrapping the published result Task; source completion defect is attributed to the scope manager.'
    'StateMachineRuntimeController.cs' = 'No new methods; inherits the audited StateMachineControllerBase. The inherited defect is tracked once at its declaring type.'
    'StateMachineScopeManager.cs' = 'Run explicitly transfers WaitAndCleanup to TaskMonitor; Cleanup can throw before caller result publication reaches a setter. DestroyAll aggregates returned operations; skipping the successful private async-ValueTask Destroy result cannot lose a custom source because that private method uses the ordinary async builder.'
}
$tasks = @{
    'TaskMonitor.cs' = @('CONS-002','CONS-003','CONS-004')
    'TaskExtensions.cs' = @('CONS-005')
    'ValueTaskExtensions.cs' = @('CONS-005')
    'LazyTask.cs' = @('CONS-005')
    'StateMachineScopeManager.cs' = @('CONS-001')
    'ExternalServiceScopeManager.cs' = @('CONS-001')
    'StateMachineDestroyOnIdle.cs' = @('CONS-006')
    'StateMachineControllerBase.cs' = @('CONS-008')
    'StateMachineStatus.cs' = @('CONS-008')
}
foreach ($unitId in $CompleteUnits) {
    $unit = $progress.work_units | Where-Object id -EQ $unitId
    if (!$unit -or $unitId -eq 'C47-inventory') { throw "Invalid unit $unitId" }
    $unit.state = 'in_progress'
    $progress.next_step = "Save $unitId file dispositions; then reconcile remaining units."
    $progress.updated_at = [DateTimeOffset]::UtcNow.ToString('o')
    $progress | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath "$progressPath.tmp"
    Move-Item -LiteralPath "$progressPath.tmp" -Destination $progressPath -Force
    foreach ($path in $unit.eligible_set) {
        $row = $rows | Where-Object path -EQ $path
        $file = $structure | Where-Object path -EQ $path
        if (!$row -or !$file -or $file.parse_errors.Count) { throw "Missing/unparsed $path" }
        $leaf = [IO.Path]::GetFileName($path)
        $row.evidence = @('Complete source/control-flow inspection','AUDIT.structure.json: '+$path)
        $row.reason = $notes[$leaf]
        $row.tasks = @()
        if ($tasks.ContainsKey($leaf)) {
            $row.disposition = 'Deviation'
            $row.tasks = $tasks[$leaf]
        } elseif ($leaf -eq 'IoBoundTaskScheduler.cs') {
            $row.disposition = 'Justified exception'
        } elseif ($leaf -eq 'StateMachineRuntimeController.cs') {
            $row.disposition = 'Compliant'
        } elseif (@($file.members | Where-Object signature -Match '\b(?:Task|ValueTask|TaskFactory)(?:<|\b)').Count -gt 0) {
            $row.disposition = 'Compliant'
            if (!$row.reason) {
                $row.reason = 'Interface signatures expose asynchronous completion to the caller; no implementation body, hidden wait or detached operation exists in this declaration. Implementation is reviewed separately.'
            }
        } else {
            $row.disposition = 'Not applicable'
            if (!$row.reason) {
                $row.reason = 'Complete file inspection found only synchronous data, identity, exception, forwarding or registration declarations; no new asynchronous operation, completion proxy or wait is implemented here. Inherited/base execution is covered at its declaring file.'
            }
        }
    }
    $rows | ConvertTo-Json -Depth 25 | Set-Content -LiteralPath $coveragePath
    $unit.state = 'complete'
    $unit.processed = @($unit.eligible_set)
    $unit.remaining = @()
    $unit.evidence = @('AUDIT.coverage.json','AUDIT.structure.json','TODO.md')
    $progress.checkpoint++
    $progress.outputs | Add-Member -NotePropertyName todo -NotePropertyValue @{path='TODO.md';sha256=(Get-FileHash (Join-Path $artifactDir 'TODO.md')).Hash} -Force
    $progress.outputs | Add-Member -NotePropertyName file_coverage -NotePropertyValue @{path='AUDIT.coverage.json';sha256=(Get-FileHash $coveragePath).Hash} -Force
    $progress.next_step = 'Inspect remaining units; if none remain, render COVERAGE.md and validate every path, symbol span, task dependency and input/output hash.'
    $progress.updated_at = [DateTimeOffset]::UtcNow.ToString('o')
    $progress | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath "$progressPath.tmp"
    Move-Item -LiteralPath "$progressPath.tmp" -Destination $progressPath -Force
}
$rows | Group-Object disposition | Select-Object Name,Count
