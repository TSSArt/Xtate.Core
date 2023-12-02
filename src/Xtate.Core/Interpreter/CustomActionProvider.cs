using System.IO;
using System.Xml;

namespace Xtate.CustomAction;

public abstract class CustomActionProvider<TCustomAction> : ICustomActionProvider, ICustomActionActivator where TCustomAction : CustomActionBase
{
	private readonly string    _name;
	private readonly NameTable _nameTable = default!;

	private readonly string _ns;

	protected CustomActionProvider(string Namespace, string Name)
	{
		_ns = Namespace;
		_name = Name;
	}

	public required Func<XmlReader, TCustomAction> CustomActionFactory { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public required INameTableProvider? NameTableProvider
	{
		init
		{
			Infra.Requires(value);

			_nameTable = value.GetNameTable();

			_ns = _nameTable.Add(_ns);
			_name = _nameTable.Add(_name);
		}
	}

#region Interface ICustomActionActivator

	public virtual CustomActionBase Activate(string xml)
	{
		using var stringReader = new StringReader(xml);

		var nsManager = new XmlNamespaceManager(_nameTable);
		var context = new XmlParserContext(_nameTable, nsManager, xmlLang: null, xmlSpace: default);

		using var xmlReader = XmlReader.Create(stringReader, settings: null, context);

		xmlReader.MoveToContent();

		Infra.Assert(xmlReader.NamespaceURI == _ns);
		Infra.Assert(xmlReader.LocalName == _name);

		return CustomActionFactory(xmlReader);
	}

#endregion

#region Interface ICustomActionProvider

	public virtual ICustomActionActivator? TryGetActivator(string ns, string name) => ns == _ns && name == _name ? this : default;

#endregion
}