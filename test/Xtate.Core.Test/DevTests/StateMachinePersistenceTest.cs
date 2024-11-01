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

using System.Collections.Specialized;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Xml;
using Xtate.Core;
using Xtate.IoC;
using Xtate.Persistence;

namespace Xtate.Test;

[TestClass]
public class StateMachinePersistenceTest
{
	private IStateMachine _allStateMachine = default!;

	private Mock<IResourceLoader> _resourceLoaderServiceMock = default!;

	[TestInitialize]
	public void Initialize()
	{
		var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Xtate.UnitTest.Resources.All.xml");

		XmlNameTable nt = new NameTable();
		var xmlNamespaceManager = new XmlNamespaceManager(nt);
		using var xmlReader = XmlReader.Create(stream!, settings: null, new XmlParserContext(nt, xmlNamespaceManager, xmlLang: default, xmlSpace: default));

		//var director = serviceLocator.GetService<ScxmlDirector, XmlReader>(xmlReader);
		//var director = new ScxmlDirector(xmlReader, serviceLocator.GetService<IBuilderFactory>(), new ScxmlDirectorOptions(serviceLocator) { NamespaceResolver = xmlNamespaceManager });

		var sc = new ServiceCollection();
		sc.AddModule<StateMachineFactoryModule>();
		sc.AddConstant<IStateMachineLocation>(new LocationStateMachine(new Uri("res://Xtate.UnitTest/Xtate.UnitTest/Resources/All.xml")));
		var sp = sc.BuildProvider();

		_allStateMachine = sp.GetRequiredService<IStateMachine>().Result;

		var task = new ValueTask<Resource>(new Resource(new MemoryStream("'content'"u8.ToArray()), new ContentType()));
		var loaderMock = new Mock<IResourceLoader>();
		loaderMock.Setup(e => e.Request(It.IsAny<Uri>(), It.IsAny<NameValueCollection>()))
				  .Returns(task);

		_resourceLoaderServiceMock = new Mock<IResourceLoader>();
		_resourceLoaderServiceMock.Setup(e => e.Request(It.IsAny<Uri>(), default)).Returns(task);
		_resourceLoaderServiceMock.Setup(e => e.Request(It.IsAny<Uri>(), It.IsAny<NameValueCollection>())).Returns(task);
	}

	public class TestStorage : IStorageProvider
	{
		private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, MemoryStream>> _storage = new();

	#region Interface IStorageProvider

		public async ValueTask<ITransactionalStorage> GetTransactionalStorage(string? partition, string key)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException(message: @"Value cannot be null or empty.", nameof(key));

			var partitionStorage = _storage.GetOrAdd(partition ?? "", _ => new ConcurrentDictionary<string, MemoryStream>());
			var memStream = partitionStorage.GetOrAdd(key, _ => new MemoryStream());

			var streamStorage = new StreamStorage(memStream, disposeStream: false)
								{
									InMemoryStorageFactory = b => new InMemoryStorage(b),
									InMemoryStorageBaselineFactory = memory => new InMemoryStorage(memory.Span)
								};
			await streamStorage.Initialization;

			return streamStorage;
		}

		public ValueTask RemoveTransactionalStorage(string? partition, string key)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException(message: @"Value cannot be null or empty.", nameof(key));

			var partitionStorage = _storage.GetOrAdd(partition ?? "", _ => new ConcurrentDictionary<string, MemoryStream>());
			partitionStorage.TryRemove(key, out _);

			return default;
		}

		public ValueTask RemoveAllTransactionalStorage(string? partition)
		{
			_storage.TryRemove(partition ?? "", out _);

			return default;
		}

	#endregion
	}
}