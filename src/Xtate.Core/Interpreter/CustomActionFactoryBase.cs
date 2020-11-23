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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Xtate.CustomAction
{
	public abstract class CustomActionFactoryBase : ICustomActionFactory
	{
		private Activator? _activator;

	#region Interface ICustomActionFactory

		ValueTask<ICustomActionFactoryActivator?> ICustomActionFactory.TryGetActivator(IFactoryContext factoryContext, string ns, string name, CancellationToken token)
		{
			_activator ??= CreateActivator();

			return new ValueTask<ICustomActionFactoryActivator?>(_activator.CanHandle(ns, name) ? _activator : null);
		}

	#endregion

		private Activator CreateActivator()
		{
			var catalog = new Catalog();

			Register(catalog);

			return new Activator(catalog);
		}

		protected abstract void Register(ICustomActionCatalog catalog);

		private class Catalog : ICustomActionCatalog
		{
			private readonly Dictionary<(string ns, string name), Delegate> _creators = new();

		#region Interface ICustomActionCatalog

			public void Register(string ns, string name, ICustomActionCatalog.Creator creator)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));
				if (creator is null) throw new ArgumentNullException(nameof(creator));

				_creators.Add((ns, name), creator);
			}

			public void Register(string ns, string name, ICustomActionCatalog.ExecutorCreator creator)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));
				if (creator is null) throw new ArgumentNullException(nameof(creator));

				_creators.Add((ns, name), creator);
			}

			public void Register(string ns, string name, ICustomActionCatalog.ExecutorCreatorAsync creator)
			{
				if (ns is null) throw new ArgumentNullException(nameof(ns));
				if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));
				if (creator is null) throw new ArgumentNullException(nameof(creator));

				_creators.Add((ns, name), creator);
			}

		#endregion

			public bool CanHandle(string ns, string name) => _creators.ContainsKey((ns, name));

			public ValueTask<ICustomActionExecutor> CreateExecutor(string ns, string name, IFactoryContext factoryContext, ICustomActionContext context, XmlReader reader, CancellationToken token)
			{
				switch (_creators[(ns, name)])
				{
					case ICustomActionCatalog.Creator creator:
						var executor = creator();
						executor.SetContextAndInitialize(context, reader);

						return new ValueTask<ICustomActionExecutor>(executor);

					case ICustomActionCatalog.ExecutorCreator creator:
						return new ValueTask<ICustomActionExecutor>(creator(context, reader));

					case ICustomActionCatalog.ExecutorCreatorAsync creator:
						return creator(factoryContext, context, reader, token);

					default:
						return Infrastructure.UnexpectedValue<ValueTask<ICustomActionExecutor>>(_creators[(ns, name)]?.GetType());
				}
			}
		}

		private class Activator : ICustomActionFactoryActivator
		{
			private readonly Catalog _catalog;

			public Activator(Catalog catalog) => _catalog = catalog;

		#region Interface ICustomActionFactoryActivator

			public ValueTask<ICustomActionExecutor> CreateExecutor(IFactoryContext factoryContext, ICustomActionContext customActionContext, CancellationToken token)
			{
				if (customActionContext is null) throw new ArgumentNullException(nameof(customActionContext));

				using var stringReader = new StringReader(customActionContext.Xml);

				var nameTable = new NameTable();
				var nsManager = new XmlNamespaceManager(nameTable);
				var context = new XmlParserContext(nameTable, nsManager, xmlLang: null, xmlSpace: default);

				using var xmlReader = XmlReader.Create(stringReader, settings: null, context);

				xmlReader.MoveToContent();

				var ns = customActionContext.XmlNamespace;
				var name = customActionContext.XmlName;

				Infrastructure.Assert(xmlReader.NamespaceURI == ns);
				Infrastructure.Assert(xmlReader.LocalName == name);

				return _catalog.CreateExecutor(ns, name, factoryContext, customActionContext, xmlReader, token);
			}

		#endregion

			public bool CanHandle(string ns, string name) => _catalog.CanHandle(ns, name);
		}
	}
}