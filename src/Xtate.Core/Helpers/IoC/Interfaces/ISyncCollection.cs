namespace Xtate.Core;

[CollectionBuilder(typeof(ServiceSyncListBuilder), nameof(ServiceSyncListBuilder.Create))]
public interface ISyncCollection<out T> : IReadOnlyCollection<T>;