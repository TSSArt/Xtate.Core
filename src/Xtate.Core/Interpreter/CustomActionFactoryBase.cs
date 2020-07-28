#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Xtate.CustomAction
{
	public abstract class CustomActionFactoryBase : ICustomActionFactory
	{
		private readonly Dictionary<string, Func<XmlReader, ICustomActionContext, ICustomActionExecutor>> _actions =
				new Dictionary<string, Func<XmlReader, ICustomActionContext, ICustomActionExecutor>>();

		private readonly string _namespace;

		protected CustomActionFactoryBase()
		{
			var customActionProviderAttribute = GetType().GetCustomAttribute<CustomActionProviderAttribute>();

			if (customActionProviderAttribute == null)
			{
				throw new InfrastructureException(Res.Format(Resources.Exception_CustomActionProviderAttributeWasNotProvided, GetType()));
			}

			_namespace = customActionProviderAttribute.Namespace;
		}

	#region Interface ICustomActionFactory

		public bool CanHandle(string ns, string name) => ns == _namespace && _actions.ContainsKey(name);

		public ICustomActionExecutor CreateExecutor(ICustomActionContext customActionContext)
		{
			if (customActionContext == null) throw new ArgumentNullException(nameof(customActionContext));

			using var stringReader = new StringReader(customActionContext.Xml);

			var nameTable = new NameTable();
			var nsManager = new XmlNamespaceManager(nameTable);
			var context = new XmlParserContext(nameTable, nsManager, xmlLang: null, xmlSpace: default);

			using var xmlReader = XmlReader.Create(stringReader, settings: null, context);

			xmlReader.MoveToContent();

			return _actions[xmlReader.LocalName](xmlReader, customActionContext);
		}

	#endregion

		protected void Register(string name, Func<XmlReader, ICustomActionContext, ICustomActionExecutor> executorFactory)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (executorFactory == null) throw new ArgumentNullException(nameof(executorFactory));

			_actions.Add(name, executorFactory);
		}
	}
}