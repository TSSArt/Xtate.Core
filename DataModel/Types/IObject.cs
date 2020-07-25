using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public interface IObject
	{
		object? ToObject();
	}
}