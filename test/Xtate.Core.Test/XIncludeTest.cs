#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.XInclude;

namespace Xtate.Core.Test
{
	[TestClass]
	public class XIncludeTest
	{
		[TestMethod]
		public async Task CreateStateMachineWithXInclude()
		{
			var host = new StateMachineHostBuilder()
					   .AddResourceLoaderFactory(ResxResourceLoaderFactory.Instance)
					   .Build();

			await host.StartHostAsync();

			var _ = await host.ExecuteStateMachineAsync(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeSource.scxml"));

			await host.StopHostAsync();
		}

		[TestMethod]
		public async Task DtdReaderTest()
		{
			var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/DtdSingleIncludeSource.scxml");

			var securityContext = SecurityContext.Create(SecurityContextType.NewStateMachine, new DeferredFinalizer());
			var proxy = new RedirectXmlResolver(ImmutableArray.Create(ResxResourceLoaderFactory.Instance), securityContext, token: default);
			var factoryContext = new FactoryContext(ImmutableArray.Create(ResxResourceLoaderFactory.Instance), securityContext);
			var resource = await factoryContext.GetResource(uri, token: default);
			var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = proxy, DtdProcessing = DtdProcessing.Parse };
			var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true, token: default), xmlReaderSettings, uri.ToString());

			//var xIncludeReader = new XIncludeReader(xmlReader, xmlReaderSettings, proxy, maxNestingLevel: 0);

			var builder = new StringBuilder();
			var xmlWriter = XmlWriter.Create(builder);
			while (await xmlReader.ReadAsync())
			{
				// ReSharper disable once MethodHasAsyncOverload
				xmlWriter.WriteNode(xmlReader, defattr: false);
			}

			xmlWriter.Close();

			Console.Write(builder.ToString());
		}

		[TestMethod]
		public async Task XIncludeReaderTest()
		{
			var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeSource.scxml");
			
			var securityContext = SecurityContext.Create(SecurityContextType.NewStateMachine, new DeferredFinalizer());
			var proxy = new RedirectXmlResolver(ImmutableArray.Create(ResxResourceLoaderFactory.Instance), securityContext, token: default);
			var factoryContext = new FactoryContext(ImmutableArray.Create(ResxResourceLoaderFactory.Instance), securityContext);

			var resource = await factoryContext.GetResource(uri, headers: default, token: default);
			var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = proxy };
			var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true, token: default), xmlReaderSettings, uri.ToString());
			var xIncludeReader = new XIncludeReader(xmlReader, xmlReaderSettings, proxy, maxNestingLevel: 0);

			var builder = new StringBuilder();
			var xmlWriter = XmlWriter.Create(builder);
			while (await xIncludeReader.ReadAsync())
			{
				// ReSharper disable once MethodHasAsyncOverload
				xmlWriter.WriteNode(xIncludeReader, defattr: false);
			}

			xmlWriter.Close();

			Console.Write(builder.ToString());
		}
	}
}