// Audit-only collaborators. Actual production TaskMonitor, LazyTask and extensions
// are linked by AUDIT.probe.csproj, never copied or modified.
global using System.Collections.Concurrent;
global using System.Threading.Tasks;
global using System.Threading;

namespace Xtate
{
    [AttributeUsage(AttributeTargets.Class)] public sealed class InstantiatedByIoCAttribute : Attribute;
    [AttributeUsage(AttributeTargets.Method)] public sealed class SetByIoCAttribute : Attribute;
    public static class Resources
    {
        public const string Message_TaskWasCanceled = "cancelled";
        public const string Message_TaskFailed = "failed";
    }
    public static class Infra
    {
        public static void NotNull([System.Diagnostics.CodeAnalysis.NotNull] object? value)
        {
            if (value is null) throw new InvalidOperationException();
        }
    }
}
namespace Xtate.Logging
{
    public enum Level { Warning, Error }
    public interface ILogger<T>
    {
        ValueTask Write(Level level, int eventId, string? message, Exception entity);
    }
}
namespace Xtate.DataTypes { public readonly struct DataModelValue; }
namespace Xtate.DataModel { public interface IIncomingEvent; }
namespace Xtate.Interpreter
{
    public enum StateMachineInterpreterState { Initializing, Accepted }
    public interface INotifyStateChanged { ValueTask OnChanged(StateMachineInterpreterState state); }
    public interface IEventDispatcher { ValueTask Dispatch(Xtate.DataModel.IIncomingEvent item, CancellationToken token); }
    public interface IStateMachineInterpreter
    {
        ValueTask<Xtate.DataTypes.DataModelValue> Run();
        void TriggerDestroySignal();
    }
    public sealed class StateMachineDestroyedException : Exception;
}
namespace Xtate.StateMachineHost
{
    public interface IStateMachineController : Xtate.Interpreter.IEventDispatcher
    {
        ValueTask<Xtate.DataTypes.DataModelValue> GetResult();
        ValueTask Destroy();
    }
}
namespace Xtate.IoC
{
    public interface IAsyncInitialization { ValueTask InitializeAsync(); }
    // The startup probe calls the actual protected Start directly. These IoC
    // placeholders only satisfy compilation; they do not model container behavior.
    public sealed class AsyncInit<T>(Func<T, ValueTask> action)
    {
        internal Func<T, ValueTask> Action = action;
        public void EnsureInitialized() => throw new InvalidOperationException("IoC not exercised by audit probe");
    }
    public static class AsyncInit
    {
        public static Runner<T> For<T>(T instance) => new(instance);
        public readonly struct Runner<T>(T instance)
        {
            public ValueTask Run(AsyncInit<T> entry) => entry.Action(instance);
        }
    }
}
