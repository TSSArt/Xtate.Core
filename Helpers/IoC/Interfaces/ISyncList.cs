namespace Xtate.Core;

[CollectionBuilder(typeof(ServiceSyncListBuilder), nameof(ServiceSyncListBuilder.Create))]
public interface ISyncList<out T> : IReadOnlyList<T>, ISyncCollection<T>;