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
	private readonly IValueEvaluator?                    _contentBodyEvaluator;
	private readonly IObjectEvaluator?                   _contentExpressionEvaluator;
	private readonly ILocationEvaluator?                 _idLocationEvaluator;
	private readonly IInvoke                             _invoke;
	private readonly ImmutableArray<ILocationEvaluator>  _nameEvaluatorList;
	private readonly ImmutableArray<DataConverter.Param> _parameterList;
	private readonly IStringEvaluator?                   _sourceExpressionEvaluator;
	private readonly IStringEvaluator?                   _typeExpressionEvaluator;
	private          DocumentIdSlot                      _documentIdSlot;
	private          StateEntityNode?                    _source;

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
		_parameterList = DataConverter.AsParamArray(invoke.Parameters);

		Finalize = invoke.Finalize?.As<FinalizeNode>();
	}

	public required Func<ValueTask<DataConverter>>      DataConverterFactory    { private get; [UsedImplicitly] init; }
	public required Func<ValueTask<IInvokeController>> InvokeControllerFactory { private get; [UsedImplicitly] init; }
	public InvokeId? InvokeId { get; private set; }

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

	public Uri?                                Type             => _invoke.Type;
	public IValueExpression?                   TypeExpression   => _invoke.TypeExpression;
	public Uri?                                Source           => _invoke.Source;
	public IValueExpression?                   SourceExpression => _invoke.SourceExpression;
	public string?                             Id               => _invoke.Id;
	public ILocationExpression?                IdLocation       => _invoke.IdLocation;
	public bool                                AutoForward      => _invoke.AutoForward;
	public ImmutableArray<ILocationExpression> NameList         => _invoke.NameList;
	public ImmutableArray<IParam>              Parameters       => _invoke.Parameters;
	public IContent?                           Content          => _invoke.Content;
	IFinalize? IInvoke.                        Finalize         => _invoke.Finalize;

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

	public void SetSource(StateEntityNode source) => _source = source;

	public async ValueTask Start()
	{
		Infra.NotNull(_source, Resources.Exception_SourceNotInitialized);

		InvokeId = InvokeId.New(_source.Id, _invoke.Id);

		if (_idLocationEvaluator is not null)
		{
			await _idLocationEvaluator.SetValue(InvokeId).ConfigureAwait(false);
		}

		var type = _typeExpressionEvaluator is not null ? ToUri(await _typeExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : _invoke.Type;
		var source = _sourceExpressionEvaluator is not null ? ToUri(await _sourceExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : _invoke.Source;
		var rawContent = _contentBodyEvaluator is IStringEvaluator rawContentEvaluator ? await rawContentEvaluator.EvaluateString().ConfigureAwait(false) : null;

		var dataConverter = await DataConverterFactory().ConfigureAwait(false);
		var content = await dataConverter.GetContent(_contentBodyEvaluator, _contentExpressionEvaluator).ConfigureAwait(false);
		var parameters = await dataConverter.GetParameters(_nameEvaluatorList, _parameterList).ConfigureAwait(false);

		Infra.NotNull(type);

		var invokeData = new InvokeData(InvokeId, type)
						 {
							 Source = source,
							 RawContent = rawContent,
							 Content = content,
							 Parameters = parameters
						 };

		var invokeController = await InvokeControllerFactory().ConfigureAwait(false);
	
		await invokeController.Start(invokeData).ConfigureAwait(false);
	}

	private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);

	public async ValueTask Cancel()
	{
		var tmpInvokeId = InvokeId;
		InvokeId = default;

		if (tmpInvokeId is not null)
		{
			var invokeController = await InvokeControllerFactory().ConfigureAwait(false);

			await invokeController.Cancel(tmpInvokeId).ConfigureAwait(false);
		}
	}

	public async ValueTask Forward(IEvent evt)
	{
		Infra.NotNull(InvokeId);

		var invokeController = await InvokeControllerFactory().ConfigureAwait(false);
	
		await invokeController.Forward(InvokeId, evt).ConfigureAwait(false);
	}
}