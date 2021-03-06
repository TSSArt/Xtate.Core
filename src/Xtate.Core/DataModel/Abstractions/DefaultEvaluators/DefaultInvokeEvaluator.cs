﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultInvokeEvaluator : IInvoke, IStartInvokeEvaluator, ICancelInvokeEvaluator, IAncestorProvider
	{
		private readonly IInvoke _invoke;

		public DefaultInvokeEvaluator(IInvoke invoke)
		{
			_invoke = invoke ?? throw new ArgumentNullException(nameof(invoke));

			TypeExpressionEvaluator = invoke.TypeExpression?.As<IStringEvaluator>();
			SourceExpressionEvaluator = invoke.SourceExpression?.As<IStringEvaluator>();
			ContentExpressionEvaluator = invoke.Content?.Expression?.As<IObjectEvaluator>();
			ContentBodyEvaluator = invoke.Content?.Body?.As<IValueEvaluator>();
			IdLocationEvaluator = invoke.IdLocation?.As<ILocationEvaluator>();
			NameEvaluatorList = invoke.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
			ParameterList = invoke.Parameters.AsArrayOf<IParam, DefaultParam>();
		}

		public IObjectEvaluator?                  ContentExpressionEvaluator { get; }
		public IValueEvaluator?                   ContentBodyEvaluator       { get; }
		public ILocationEvaluator?                IdLocationEvaluator        { get; }
		public ImmutableArray<ILocationEvaluator> NameEvaluatorList          { get; }
		public ImmutableArray<DefaultParam>       ParameterList              { get; }
		public IStringEvaluator?                  SourceExpressionEvaluator  { get; }
		public IStringEvaluator?                  TypeExpressionEvaluator    { get; }

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _invoke;

	#endregion

	#region Interface ICancelInvokeEvaluator

		public virtual ValueTask Cancel(InvokeId invokeId, IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));
			if (invokeId is null) throw new ArgumentNullException(nameof(invokeId));

			return executionContext.CancelInvoke(invokeId, token);
		}

	#endregion

	#region Interface IInvoke

		public Uri?                                Type             => _invoke.Type;
		public IValueExpression?                   TypeExpression   => _invoke.TypeExpression;
		public Uri?                                Source           => _invoke.Source;
		public IValueExpression?                   SourceExpression => _invoke.SourceExpression;
		public string?                             Id               => _invoke.Id;
		public ILocationExpression?                IdLocation       => _invoke.IdLocation;
		public ImmutableArray<ILocationExpression> NameList         => _invoke.NameList;
		public bool                                AutoForward      => _invoke.AutoForward;
		public ImmutableArray<IParam>              Parameters       => _invoke.Parameters;
		public IFinalize?                          Finalize         => _invoke.Finalize;
		public IContent?                           Content          => _invoke.Content;

	#endregion

	#region Interface IStartInvokeEvaluator

		public virtual async ValueTask<InvokeId> Start(IIdentifier stateId, IExecutionContext executionContext, CancellationToken token)
		{
			if (stateId is null) throw new ArgumentNullException(nameof(stateId));
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			var invokeId = InvokeId.New(stateId, _invoke.Id);

			if (IdLocationEvaluator is not null)
			{
				await IdLocationEvaluator.SetValue(invokeId, executionContext, token).ConfigureAwait(false);
			}

			var type = TypeExpressionEvaluator is not null ? ToUri(await TypeExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false)) : _invoke.Type;
			var source = SourceExpressionEvaluator is not null ? ToUri(await SourceExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false)) : _invoke.Source;

			var rawContent = ContentBodyEvaluator is IStringEvaluator rawContentEvaluator ? await rawContentEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false) : null;
			var content = await DataConverter.GetContent(ContentBodyEvaluator, ContentExpressionEvaluator, executionContext, token).ConfigureAwait(false);
			var parameters = await DataConverter.GetParameters(NameEvaluatorList, ParameterList, executionContext, token).ConfigureAwait(false);

			Infra.NotNull(type);

			var invokeData = new InvokeData(invokeId, type)
							 {
								 Source = source,
								 RawContent = rawContent,
								 Content = content,
								 Parameters = parameters
							 };

			await executionContext.StartInvoke(invokeData, token).ConfigureAwait(false);

			return invokeId;
		}

	#endregion

		private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);
	}
}