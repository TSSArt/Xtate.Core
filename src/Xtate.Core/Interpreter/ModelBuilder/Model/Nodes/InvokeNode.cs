// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Ancestor;
using Xtate.Ancestor.Extensions;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.Interpreter.Internal;
using Xtate.IoC.Tools;
using Xtate.StateMachine;
using Xtate.StateMachine.Internal;

namespace Xtate.Interpreter.Model;

[InstantiatedByIoC]
public class InvokeNode : IInvoke, IAncestorProvider, IDocumentId, IDebugEntityId
{
	private readonly IValueEvaluator? _contentBodyEvaluator;

	private readonly IObjectEvaluator? _contentExpressionEvaluator;

	private readonly ILocationEvaluator? _idLocationEvaluator;

	private readonly IInvoke _invoke;

	private readonly ImmutableArray<ILocationEvaluator> _nameEvaluatorList;

	private readonly ImmutableArray<DataConverter.Param> _parameterList;

	private readonly IStringEvaluator? _sourceExpressionEvaluator;

	private readonly IStringEvaluator? _typeExpressionEvaluator;

	private DocumentIdSlot _documentIdSlot;

	public InvokeNode(DocumentIdNode documentIdNode, IInvoke invoke)
	{
		_invoke = invoke;

		documentIdNode.SaveToSlot(out _documentIdSlot);

		_typeExpressionEvaluator = invoke.TypeExpression?.UseAncestor.As<IStringEvaluator>();
		_sourceExpressionEvaluator = invoke.SourceExpression?.UseAncestor.As<IStringEvaluator>();
		_contentExpressionEvaluator = invoke.Content?.Expression?.UseAncestor.As<IObjectEvaluator>();
		_contentBodyEvaluator = invoke.Content?.Body?.UseAncestor.As<IValueEvaluator>();
		_idLocationEvaluator = invoke.IdLocation?.UseAncestor.As<ILocationEvaluator>();
		_nameEvaluatorList = invoke.NameList.UseAncestor.ItemsAs<ILocationEvaluator>();

		_parameterList = Xtate.DataModel.Services.DataConverter.AsParamArray(invoke.Parameters);

		Finalize = invoke.Finalize?.UseAncestor.As<FinalizeNode>();
	}

	public required Deferred<DataConverter> DataConverter { private get; [SetByIoC] init; }

	public InvokeId? CurrentInvokeId { get; set; }

	public FinalizeNode? Finalize { get; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _invoke;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

#endregion

#region Interface IDocumentId

	public int DocumentId => _documentIdSlot.CreateValue();

#endregion

#region Interface IInvoke

	public FullUri? Type => _invoke.Type;

	public IValueExpression? TypeExpression => _invoke.TypeExpression;

	public Uri? Source => _invoke.Source;

	public IValueExpression? SourceExpression => _invoke.SourceExpression;

	public string? Id => _invoke.Id;

	public ILocationExpression? IdLocation => _invoke.IdLocation;

	public bool AutoForward => _invoke.AutoForward;

	public ImmutableArray<ILocationExpression> NameList => _invoke.NameList;

	public ImmutableArray<IParam> Parameters => _invoke.Parameters;

	public IContent? Content => _invoke.Content;

	IFinalize? IInvoke.Finalize => _invoke.Finalize;

#endregion

	public async ValueTask<InvokeData> CreateInvokeData(InvokeId invokeId)
	{
		if (_idLocationEvaluator is not null)
		{
			await _idLocationEvaluator.SetValue(invokeId).ConfigureAwait(false);
		}

		var type = _typeExpressionEvaluator is not null ? new FullUri(await _typeExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : _invoke.Type;
		var source = _sourceExpressionEvaluator is not null ? new Uri(await _sourceExpressionEvaluator.EvaluateString().ConfigureAwait(false), UriKind.RelativeOrAbsolute) : _invoke.Source;
		var rawContent = _contentBodyEvaluator is IStringEvaluator rawContentEvaluator ? await rawContentEvaluator.EvaluateString().ConfigureAwait(false) : null;

		var dataConverter = await DataConverter().ConfigureAwait(false);
		var content = await dataConverter.GetContent(_contentBodyEvaluator, _contentExpressionEvaluator).ConfigureAwait(false);
		var parameters = await dataConverter.GetParameters(_nameEvaluatorList, _parameterList).ConfigureAwait(false);

		Infra.NotNull(type);

		return new InvokeData(invokeId, type, source, rawContent, content, parameters);
	}
}