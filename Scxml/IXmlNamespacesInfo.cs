using System.Collections.Immutable;

namespace Xtate.Scxml
{
	public interface IXmlNamespacesInfo
	{
		ImmutableArray<PrefixNamespace> Namespaces { get; }
	}
}