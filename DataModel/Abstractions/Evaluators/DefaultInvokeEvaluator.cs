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

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class InvokeEvaluator : IInvoke, IStartInvokeEvaluator, ICancelInvokeEvaluator, IAncestorProvider
{
	protected IInvoke _invoke;

	protected InvokeEvaluator(IInvoke invoke)
	{
		Infra.Requires(invoke);

		_invoke = invoke;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _invoke;

#endregion

#region Interface ICancelInvokeEvaluator

	public abstract ValueTask Cancel(InvokeId invokeId);

#endregion

#region Interface IInvoke

	public virtual Uri?                                Type             => _invoke.Type;
	public virtual IValueExpression?                   TypeExpression   => _invoke.TypeExpression;
	public virtual Uri?                                Source           => _invoke.Source;
	public virtual IValueExpression?                   SourceExpression => _invoke.SourceExpression;
	public virtual string?                             Id               => _invoke.Id;
	public virtual ILocationExpression?                IdLocation       => _invoke.IdLocation;
	public virtual ImmutableArray<ILocationExpression> NameList         => _invoke.NameList;
	public virtual bool                                AutoForward      => _invoke.AutoForward;
	public virtual ImmutableArray<IParam>              Parameters       => _invoke.Parameters;
	public virtual IFinalize?                          Finalize         => _invoke.Finalize;
	public virtual IContent?                           Content          => _invoke.Content;

#endregion

#region Interface IStartInvokeEvaluator

	public abstract ValueTask<InvokeId> Start(IIdentifier stateId);

#endregion
}

public class DefaultInvokeEvaluator : InvokeEvaluator
{
	public DefaultInvokeEvaluator(IInvoke invoke) : base(invoke)
	{
		TypeExpressionEvaluator = invoke.TypeExpression?.As<IStringEvaluator>();
		SourceExpressionEvaluator = invoke.SourceExpression?.As<IStringEvaluator>();
		ContentExpressionEvaluator = invoke.Content?.Expression?.As<IObjectEvaluator>();
		ContentBodyEvaluator = invoke.Content?.Body?.As<IValueEvaluator>();
		IdLocationEvaluator = invoke.IdLocation?.As<ILocationEvaluator>();
		NameEvaluatorList = invoke.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
		ParameterList = DataConverter.AsParamArray(invoke.Parameters);
	}

	public required Func<ValueTask<DataConverter>>     DataConverterFactory    { private get; init; }
	public required Func<ValueTask<IInvokeController>> InvokeControllerFactory { private get; init; }

	public IObjectEvaluator?                   ContentExpressionEvaluator { get; }
	public IValueEvaluator?                    ContentBodyEvaluator       { get; }
	public ILocationEvaluator?                 IdLocationEvaluator        { get; }
	public ImmutableArray<ILocationEvaluator>  NameEvaluatorList          { get; }
	public ImmutableArray<DataConverter.Param> ParameterList              { get; }
	public IStringEvaluator?                   SourceExpressionEvaluator  { get; }
	public IStringEvaluator?                   TypeExpressionEvaluator    { get; }

	public override async ValueTask Cancel(InvokeId invokeId)
	{
		Infra.Requires(invokeId);

		if (await InvokeControllerFactory().ConfigureAwait(false) is { } invokeController)
		{
			await invokeController.Cancel(invokeId).ConfigureAwait(false);
		}
	}

	public override async ValueTask<InvokeId> Start(IIdentifier stateId)
	{
		Infra.Requires(stateId);

		var invokeId = InvokeId.New(stateId, _invoke.Id);

		if (IdLocationEvaluator is not null)
		{
			await IdLocationEvaluator.SetValue(invokeId).ConfigureAwait(false);
		}

		var type = TypeExpressionEvaluator is not null ? ToUri(await TypeExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : _invoke.Type;
		var source = SourceExpressionEvaluator is not null ? ToUri(await SourceExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : _invoke.Source;

		var dataConverter = await DataConverterFactory().ConfigureAwait(false);
		var rawContent = ContentBodyEvaluator is IStringEvaluator rawContentEvaluator ? await rawContentEvaluator.EvaluateString().ConfigureAwait(false) : null;
		var content = await dataConverter.GetContent(ContentBodyEvaluator, ContentExpressionEvaluator).ConfigureAwait(false);
		var parameters = await dataConverter.GetParameters(NameEvaluatorList, ParameterList).ConfigureAwait(false);

		Infra.NotNull(type);

		if (await InvokeControllerFactory().ConfigureAwait(false) is { } invokeController)
		{
			var invokeData = new InvokeData(invokeId, type)
							 {
								 Source = source,
								 RawContent = rawContent,
								 Content = content,
								 Parameters = parameters
							 };

			await invokeController.Start(invokeData).ConfigureAwait(false);
		}

		return invokeId;
	}

	private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);
}