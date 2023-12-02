#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System.Xml;
using Xtate.Scxml;

namespace Xtate.Core;

public class ScxmlLocationStateMachineGetter
{
	private readonly Func<XmlReader, ValueTask<ScxmlDirector>> _scxmlDirectorFactory;
	private readonly ScxmlXmlResolver                          _scxmlXmlResolver;
	private readonly IStateMachineLocation                     _stateMachineLocation;

	public ScxmlLocationStateMachineGetter(IStateMachineLocation stateMachineLocation, ScxmlXmlResolver scxmlXmlResolver, Func<XmlReader, ValueTask<ScxmlDirector>> scxmlDirectorFactory)
	{
		_stateMachineLocation = stateMachineLocation;
		_scxmlXmlResolver = scxmlXmlResolver;
		_scxmlDirectorFactory = scxmlDirectorFactory;
	}

	public required IScxmlDeserializer _ScxmlDeserializer { private get; [UsedImplicitly] init; }

	public required IStateMachineValidator StateMachineValidator { private get; [UsedImplicitly] init; }

	public async ValueTask<IStateMachine> GetStateMachine()
	{
		using var xmlReader = CreateXmlReader();

		//var scxmlDirector = await _scxmlDirectorFactory(xmlReader).ConfigureAwait(false);
		var stateMachine = await _ScxmlDeserializer.Deserialize(xmlReader).ConfigureAwait(false);

		StateMachineValidator.Validate(stateMachine);

		return stateMachine;
	}

	protected virtual XmlReader CreateXmlReader() => XmlReader.Create(_stateMachineLocation.Location.ToString(), GetXmlReaderSettings(), GetXmlParserContext());

	protected virtual XmlReaderSettings GetXmlReaderSettings() =>
		new()
		{
			Async = true,
			XmlResolver = _scxmlXmlResolver,
			DtdProcessing = DtdProcessing.Parse
		};

	protected virtual XmlParserContext GetXmlParserContext()
	{
		var nsManager = new XmlNamespaceManager(new NameTable());

		return new XmlParserContext(nt: null, nsManager, xmlLang: null, XmlSpace.None) { BaseURI = _stateMachineLocation.Location.ToString() };
	}
	/*
		private static ScxmlDirectorOptions GetScxmlDirectorOptions(ServiceLocator serviceLocator,
																	XmlParserContext xmlParserContext,
																	XmlReaderSettings xmlReaderSettings,
																	XmlResolver xmlResolver) =>
			new(serviceLocator)
			{
				NamespaceResolver = xmlParserContext.NamespaceManager,
				XmlReaderSettings = xmlReaderSettings,
				XmlResolver = xmlResolver,
				XIncludeAllowed = true,
				Async = true
			};*/
}