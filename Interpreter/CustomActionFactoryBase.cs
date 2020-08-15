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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Xtate.CustomAction
{
	public abstract class CustomActionFactoryBase : ICustomActionFactory, ICustomActionFactoryActivator
	{
		private readonly Dictionary<string, Func<XmlReader, ICustomActionContext, ICustomActionExecutor>> _actions =
				new Dictionary<string, Func<XmlReader, ICustomActionContext, ICustomActionExecutor>>();

		private readonly string _namespace;

		protected CustomActionFactoryBase()
		{
			if (GetType().GetCustomAttribute<CustomActionProviderAttribute>() is {} customActionProviderAttribute)
			{
				_namespace = customActionProviderAttribute.Namespace;

				return;
			}

			throw new InfrastructureException(Res.Format(Resources.Exception_CustomActionProviderAttributeWasNotProvided, GetType()));
		}

	#region Interface ICustomActionFactory

		public ValueTask<ICustomActionFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, string ns, string name, CancellationToken token) =>
				new ValueTask<ICustomActionFactoryActivator?>(CanHandle(ns, name) ? this : null);

	#endregion

	#region Interface ICustomActionFactoryActivator

		public ValueTask<ICustomActionExecutor> CreateExecutor(IFactoryContext factoryContext, ICustomActionContext customActionContext, CancellationToken token)
		{
			if (customActionContext is null) throw new ArgumentNullException(nameof(customActionContext));

			Infrastructure.Assert(_namespace == customActionContext.XmlNamespace);

			using var stringReader = new StringReader(customActionContext.Xml);

			var nameTable = new NameTable();
			var nsManager = new XmlNamespaceManager(nameTable);
			var context = new XmlParserContext(nameTable, nsManager, xmlLang: null, xmlSpace: default);

			using var xmlReader = XmlReader.Create(stringReader, settings: null, context);

			xmlReader.MoveToContent();

			Infrastructure.Assert(xmlReader.NamespaceURI == customActionContext.XmlNamespace);
			Infrastructure.Assert(xmlReader.LocalName == customActionContext.XmlName);

			return new ValueTask<ICustomActionExecutor>(_actions[xmlReader.LocalName](xmlReader, customActionContext));
		}

	#endregion

		private bool CanHandle(string ns, string name) => ns == _namespace && _actions.ContainsKey(name);

		protected void Register(string name, Func<XmlReader, ICustomActionContext, ICustomActionExecutor> executorFactory)
		{
			if (name is null) throw new ArgumentNullException(nameof(name));
			if (executorFactory is null) throw new ArgumentNullException(nameof(executorFactory));

			_actions.Add(name, executorFactory);
		}
	}
}