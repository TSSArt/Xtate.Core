using System.Xml;

namespace Xtate.Core;

public interface INameTableProvider
{
	NameTable GetNameTable();
}