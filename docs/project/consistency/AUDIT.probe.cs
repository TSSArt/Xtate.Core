using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xtate;
using Monitor = Xtate.TaskMonitor.Services.TaskMonitor;

var artifactDir = Path.GetFullPath(args[0]);
var sourceRoot = Path.GetFullPath(Path.Combine(artifactDir, "../../.."));
using var inventory = JsonDocument.Parse(File.ReadAllText(Path.Combine(artifactDir, "AUDIT.inventory.json")));
var structure = inventory.RootElement.GetProperty("files").EnumerateArray().Select(file =>
{
    var path = file.GetProperty("path").GetString()!;
    var source = File.ReadAllText(Path.Combine(sourceRoot, path));
    var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
    var tree = CSharpSyntaxTree.ParseText(source, parseOptions);
    var trees = source.Contains("#if") ? new[] { tree, CSharpSyntaxTree.ParseText(source, parseOptions.WithPreprocessorSymbols("NETCOREAPP3_0_OR_GREATER")) } : new[] { tree };
    var nodes = trees.SelectMany(t => t.GetRoot().DescendantNodes()).DistinctBy(n => (n.SpanStart, n.Span.Length, n.RawKind)).ToArray();
    return new
    {
        path,
        members = nodes.Where(n => n is BaseMethodDeclarationSyntax or PropertyDeclarationSyntax or LocalFunctionStatementSyntax)
            .Select(n => new { kind = n.GetType().Name, line = tree.GetLineSpan(n.Span).StartLinePosition.Line + 1,
                end_line = tree.GetLineSpan(n.Span).EndLinePosition.Line + 1,
                signature = n.ToString().Split('\n')[0].Trim() }).ToArray(),
        awaits = nodes.OfType<AwaitExpressionSyntax>().Select(n => new { line = tree.GetLineSpan(n.Span).StartLinePosition.Line + 1, expression = n.Expression.ToString() }).ToArray(),
        invocations = nodes.OfType<InvocationExpressionSyntax>().Select(n => new { line = tree.GetLineSpan(n.Span).StartLinePosition.Line + 1, expression = n.ToString() }).ToArray(),
        conditional_variants = trees.Length == 1 ? new[] { "unconditional" } : new[] { "legacy symbols", "NETCOREAPP3_0_OR_GREATER" },
        parse_errors = trees.SelectMany(t => t.GetDiagnostics()).Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.ToString()).Distinct().ToArray()
    };
}).ToArray();
var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(artifactDir, "AUDIT.structure.json"), JsonSerializer.Serialize(structure, options));

var probes = new List<object>();
using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
var monitor = new ProbeMonitor { Logger = null! };
var pending = new Source();
var cancelledWait = monitor.WaitAsync(new ValueTask<int>(pending, 0), cancelled.Token);
try { await cancelledWait; } catch (OperationCanceledException) { }
pending.Complete(new InvalidOperationException("late"));
Check(pending.Registrations == 0 && pending.Consumptions == 0 && monitor.Failures.Count == 0, "pre_cancelled_wait_abandons_source",
    new { pending.Registrations, pending.Consumptions, logged_failures = monitor.Failures.Count });

var success = new Source();
success.Complete();
monitor.Forget(new ValueTask<int>(success, 0));
Check(success.Consumptions == 0, "successful_valuetask_forget_skips_getresult", new { success.Consumptions });

var reporting = new ThrowingReporter { Logger = null! };
var method = typeof(Monitor).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
    .Single(m => m.Name == "MonitorTaskCompletion" && !m.IsGenericMethod && m.GetParameters()[0].ParameterType == typeof(Task));
var observer = (Task)method.Invoke(reporting, new object[] { Task.FromException(new InvalidOperationException("original")) })!;
try { await observer; } catch (ApplicationException) { }
Check(observer.IsFaulted && observer.Exception!.InnerException is ApplicationException, "reporter_failure_escapes_observer",
    new { observer_status = observer.Status.ToString(), note = "Probe retrieves the private observer for evidence; production discards it." });

var delayed = new TaskCompletionSource<int>();
var lazy = new LazyTask<int>(() => new ValueTask<int>(delayed.Task), taskMonitor: null, cancelled.Token);
var result = lazy.Task;
delayed.SetException(new InvalidOperationException("late factory failure"));
Check(result.IsCanceled, "lazy_result_cannot_receive_post_cancellation_fault",
    new { result_status = result.Status.ToString(), note = "Factory fault goes through Execute rethrow and the unmonitored Forget/Preserve path, established by linked source control flow." });

var readiness = new TrackingStatus();
var startupFailure = new InvalidOperationException("startup failure");
var controller = new StartupController
{
    StateMachineInterpreter = new FailingInterpreter(startupFailure),
    StateMachineStatus = readiness,
    TaskMonitor = monitor,
    EventDispatcher = null!
};
try { await controller.StartForAudit(); } catch (InvalidOperationException error) when (ReferenceEquals(error, startupFailure)) { }
var proxy = typeof(Xtate.StateMachineHost.Services.StateMachineControllerBase)
    .GetField("_completedTcs", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(controller)!;
var privateResult = (Task)proxy.GetType().GetProperty("Task")!.GetValue(proxy)!;
Check(privateResult.IsFaulted && readiness.AcceptWaits == 0 && readiness.WhenAcceptedForAudit().IsFaulted,
    "synchronous_startup_failure_leaves_separate_faulted_proxies",
    new { result_status = privateResult.Status.ToString(), readiness_waits = readiness.AcceptWaits,
        note = "Actual Start/ExecuteAsync/status sources; IoC initialization is not exercised. Audit now observes both proxy exceptions explicitly." });
_ = privateResult.Exception;
_ = readiness.WhenAcceptedForAudit().Exception;

File.WriteAllText(Path.Combine(artifactDir, "AUDIT.probes.json"), JsonSerializer.Serialize(new
{
    runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    boundary = "Source-linked production task monitor, LazyTask, extensions, controller Start/ExecuteAsync and status with audit-only collaborators. IoC container behavior is not exercised. These reproduce current behavior; they are not passing regression tests for remediation or a full integration run.",
    probes,
    structural_counts = new { files = structure.Length, members = structure.Sum(f => f.members.Length), awaits = structure.Sum(f => f.awaits.Length), parse_errors = structure.Sum(f => f.parse_errors.Length) }
}, options));
Console.WriteLine(JsonSerializer.Serialize(new { probes = probes.Count, files = structure.Length, parse_errors = structure.Sum(f => f.parse_errors.Length) }));

void Check(bool condition, string id, object evidence)
{
    if (!condition) throw new Exception($"Reproduction did not establish {id}");
    probes.Add(new { id, current_deviation_reproduced = true, evidence });
}
sealed class Source : IValueTaskSource<int>
{
    private ManualResetValueTaskSourceCore<int> _core;
    public int Registrations;
    public int Consumptions;
    public void Complete(Exception? error = null) { if (error is null) _core.SetResult(1); else _core.SetException(error); }
    public int GetResult(short token) { Consumptions++; return _core.GetResult(token); }
    public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    { Registrations++; _core.OnCompleted(continuation, state, token, flags); }
}
sealed class ProbeMonitor : Monitor
{
    public List<Exception> Failures = [];
    protected override ValueTask TaskFailed(Exception ex) { Failures.Add(ex); return default; }
    protected override ValueTask TaskCancelled(OperationCanceledException ex) => default;
}
sealed class ThrowingReporter : Monitor
{
    protected override ValueTask TaskFailed(Exception ex) => ValueTask.FromException(new ApplicationException("reporter failure", ex));
}
sealed class StartupController : Xtate.StateMachineHost.Services.StateMachineControllerBase
{
    public ValueTask StartForAudit() => base.Start();
}
sealed class FailingInterpreter(Exception error) : Xtate.Interpreter.IStateMachineInterpreter
{
    public ValueTask<Xtate.DataTypes.DataModelValue> Run() => ValueTask.FromException<Xtate.DataTypes.DataModelValue>(error);
    public void TriggerDestroySignal() { }
}
sealed class TrackingStatus : Xtate.StateMachineHost.StateMachineStatus, Xtate.StateMachineHost.IStateMachineStatus
{
    public int AcceptWaits;
    public new Task WhenAccepted() { AcceptWaits++; return base.WhenAccepted(); }
    public Task WhenAcceptedForAudit() => base.WhenAccepted();
}
