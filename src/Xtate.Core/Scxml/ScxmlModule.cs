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
using Xtate.Builder;
using Xtate.IoC;
using Xtate.XInclude;

namespace Xtate.Scxml;

public class ScxmlModule : Module<ErrorProcessorModule, StateMachineBuilderModule, ResourceLoadersModule, NameTableModule>
{
	protected override void AddServices()
	{
		Services.AddType<ScxmlSerializerWriter, XmlWriter>();
		Services.AddImplementation<ScxmlSerializer>().For<IScxmlSerializer>();
		Services.AddType<ScxmlDirector, XmlReader>();
		Services.AddTypeSync<XmlBaseReader, XmlReader>();
		Services.AddTypeSync<XIncludeReader, XmlReader>();
		Services.AddImplementationSync<RedirectXmlResolver>().For<ScxmlXmlResolver>().For<XmlResolver>().For<IXIncludeXmlResolver>();
		Services.AddImplementation<ScxmlDeserializer>().For<IScxmlDeserializer>();
	}
}