using Xtate.Annotations;

namespace Xtate.Persistence
{
	[PublicAPI]
	internal interface IPersistedDocumentId
	{
		int DocumentId { get; }
	}
}