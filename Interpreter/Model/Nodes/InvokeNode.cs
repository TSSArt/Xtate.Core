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

using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public class InvokeNode : IInvoke, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
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

		_typeExpressionEvaluator = invoke.TypeExpression?.As<IStringEvaluator>();
		_sourceExpressionEvaluator = invoke.SourceExpression?.As<IStringEvaluator>();
		_contentExpressionEvaluator = invoke.Content?.Expression?.As<IObjectEvaluator>();
		_contentBodyEvaluator = invoke.Content?.Body?.As<IValueEvaluator>();
		_idLocationEvaluator = invoke.IdLocation?.As<ILocationEvaluator>();
		_nameEvaluatorList = invoke.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
		_parameterList = DataModel.DataConverter.AsParamArray(invoke.Parameters);

		Finalize = invoke.Finalize?.As<FinalizeNode>();
	}

	public required Deferred<DataConverter> DataConverter { private get; [UsedImplicitly] init; }

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

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.InvokeNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.Add(Key.Id, Id);
		bucket.Add(Key.Type, Type);
		bucket.Add(Key.Source, Source);
		bucket.Add(Key.AutoForward, AutoForward);
		bucket.AddEntity(Key.TypeExpression, TypeExpression);
		bucket.AddEntity(Key.SourceExpression, SourceExpression);
		bucket.AddEntity(Key.IdLocation, IdLocation);
		bucket.AddEntityList(Key.NameList, NameList);
		bucket.AddEntityList(Key.Parameters, Parameters);
		bucket.AddEntity(Key.Finalize, Finalize);
		bucket.AddEntity(Key.Content, Content);
	}

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