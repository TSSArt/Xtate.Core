using System.Xml;

namespace Xtate.Core;

public class NameTableProvider : INameTableProvider
{
	private readonly NameTable _nameTable = new();

	public NameTable GetNameTable() => _nameTable;
}