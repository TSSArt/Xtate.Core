#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultCustomActionEvaluator : ICustomAction, IExecEvaluator, IAncestorProvider
	{
		private readonly CustomActionEntity            _customAction;
		private readonly ICustomActionDispatcher _customActionDispatcher;

		public DefaultCustomActionEvaluator(in CustomActionEntity customAction)
		{
			_customAction = customAction;

			var customActionDispatcher = customAction.Ancestor?.As<ICustomActionDispatcher>();

			Infrastructure.Assert(customActionDispatcher != null, Resources.Assertion_CustomActionDoesNotConfigured);

			var locations = customAction.Locations.AsArrayOf<ILocationExpression, ILocationEvaluator>(true);
			var values = customAction.Values.AsArrayOf<IValueExpression, IObjectEvaluator>(true);

			customActionDispatcher.SetEvaluators(locations, values);

			_customActionDispatcher = customActionDispatcher;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _customAction.Ancestor;

	#endregion

	#region Interface ICustomAction

		public string? XmlNamespace => _customAction.XmlNamespace;

		public string? XmlName => _customAction.XmlName;

		public string? Xml => _customAction.Xml;

		public ImmutableArray<ILocationExpression> Locations => _customAction.Locations;

		public ImmutableArray<IValueExpression> Values => _customAction.Values;

	#endregion

	#region Interface IExecEvaluator

		public virtual ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			return _customActionDispatcher.Execute(executionContext, token);
		}

	#endregion
	}
}