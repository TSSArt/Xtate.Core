// Copyright © 2019-2024 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Xml;
using Xtate.Scxml;

namespace Xtate.Core;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class ScxmlLocationStateMachineGetter
{
	private ValueTask<IStateMachine>? _stateMachine;

	public required ScxmlXmlResolver       ScxmlXmlResolver      { private get; [UsedImplicitly] init; }
	public required IStateMachineLocation  StateMachineLocation  { private get; [UsedImplicitly] init; }
	public required IScxmlDeserializer     ScxmlDeserializer     { private get; [UsedImplicitly] init; }
	public required IStateMachineValidator StateMachineValidator { private get; [UsedImplicitly] init; }

	public virtual ValueTask<IStateMachine> GetStateMachine() => _stateMachine ??= CreateStateMachine().Preserve();

	protected virtual async ValueTask<IStateMachine> CreateStateMachine()
	{
		using var xmlReader = CreateXmlReader();

		var stateMachine = await ScxmlDeserializer.Deserialize(xmlReader).ConfigureAwait(false);

		StateMachineValidator.Validate(stateMachine);

		return stateMachine;
	}

	protected virtual XmlReader CreateXmlReader() => XmlReader.Create(StateMachineLocation.Location.ToString(), GetXmlReaderSettings(), GetXmlParserContext());

	protected virtual XmlReaderSettings GetXmlReaderSettings() =>
		new()
		{
			Async = true,
			XmlResolver = ScxmlXmlResolver,
			DtdProcessing = DtdProcessing.Parse
		};

	protected virtual XmlParserContext GetXmlParserContext()
	{
		var nsManager = new XmlNamespaceManager(new NameTable());

		return new XmlParserContext(nt: null, nsManager, xmlLang: null, XmlSpace.None) { BaseURI = StateMachineLocation.Location.ToString() };
	}
}