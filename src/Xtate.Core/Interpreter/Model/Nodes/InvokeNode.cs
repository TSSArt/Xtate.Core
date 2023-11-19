#region Copyright © 2019-2021 Sergii Artemenko

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
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core
{
	public sealed class InvokeNode : IInvoke, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		public required Func<ValueTask<DataConverter>>      DataConverterFactory    { private get; init; }
		public required Func<ValueTask<IInvokeController?>> InvokeControllerFactory { private get; init; }
		public required Func<ValueTask<ILogger<IInvoke>>> LoggerFactory { private get; init; }

		private readonly ICancelInvokeEvaluator _cancelInvokeEvaluator;
		private readonly IInvoke                _invoke;
		private readonly IStartInvokeEvaluator  _startInvokeEvaluator;
		private          DocumentIdSlot         _documentIdSlot;
		private          StateEntityNode?       _source;

		public IObjectEvaluator?                  ContentExpressionEvaluator { get; }
		public IValueEvaluator?                   ContentBodyEvaluator       { get; }
		public ILocationEvaluator?                IdLocationEvaluator        { get; }
		public ImmutableArray<ILocationEvaluator> NameEvaluatorList          { get; }
		public ImmutableArray<DataConverter.Param>          ParameterList              { get; }
		public IStringEvaluator?                  SourceExpressionEvaluator  { get; }
		public IStringEvaluator?                  TypeExpressionEvaluator    { get; }


		public InvokeNode(DocumentIdNode documentIdNode, IInvoke invoke)
		{
			Infra.Requires(invoke);

			documentIdNode.SaveToSlot(out _documentIdSlot);
			
			_invoke = invoke;

			Finalize = invoke.Finalize?.As<FinalizeNode>();
			
			TypeExpressionEvaluator = invoke.TypeExpression?.As<IStringEvaluator>();
			SourceExpressionEvaluator = invoke.SourceExpression?.As<IStringEvaluator>();
			ContentExpressionEvaluator = invoke.Content?.Expression?.As<IObjectEvaluator>();
			ContentBodyEvaluator = invoke.Content?.Body?.As<IValueEvaluator>();
			IdLocationEvaluator = invoke.IdLocation?.As<ILocationEvaluator>();
			NameEvaluatorList = invoke.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
			ParameterList = DataConverter.AsParamArray(invoke.Parameters);
			//TODO: delete
			/*
			var startInvokeEvaluator = invoke.As<IStartInvokeEvaluator>();
			Infra.NotNull(startInvokeEvaluator);
			_startInvokeEvaluator = startInvokeEvaluator;

			var cancelInvokeEvaluator = invoke.As<ICancelInvokeEvaluator>();
			Infra.NotNull(cancelInvokeEvaluator);
			_cancelInvokeEvaluator = cancelInvokeEvaluator;
			*/
		}

		public InvokeId? InvokeId { get; private set; }

		public FinalizeNode? Finalize { get; }

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _invoke;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdSlot.Value;

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

			if (IdLocationEvaluator is not null)
			{
				await IdLocationEvaluator.SetValue(InvokeId).ConfigureAwait(false);
			}

			var type = TypeExpressionEvaluator is not null ? ToUri(await TypeExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : _invoke.Type;
			var source = SourceExpressionEvaluator is not null ? ToUri(await SourceExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : _invoke.Source;

			var rawContent = ContentBodyEvaluator is IStringEvaluator rawContentEvaluator ? await rawContentEvaluator.EvaluateString().ConfigureAwait(false) : null;

			var dataConverter = await DataConverterFactory().ConfigureAwait(false);
			var content = await dataConverter.GetContent(ContentBodyEvaluator, ContentExpressionEvaluator).ConfigureAwait(false);
			var parameters = await dataConverter.GetParameters(NameEvaluatorList, ParameterList).ConfigureAwait(false);

			Infra.NotNull(type);

			var invokeData = new InvokeData(InvokeId, type)
							 {
								 Source = source,
								 RawContent = rawContent,
								 Content = content,
								 Parameters = parameters
							 };

			var logger = await LoggerFactory().ConfigureAwait(false);
			await logger.Write(Level.Trace, $@"Start invoke. InvokeId: [{InvokeId}]", invokeData).ConfigureAwait(false);

			if (await InvokeControllerFactory().ConfigureAwait(false) is { } invokeController)
			{
				await invokeController.Start(invokeData).ConfigureAwait(false);
			}
		}

		private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);

		public async ValueTask Cancel()
		{
			var logger = await LoggerFactory().ConfigureAwait(false);
			await logger.Write(Level.Trace, $@"Cancel invoke. InvokeId: [{InvokeId}]", InvokeId).ConfigureAwait(false);

			var tmpInvokeId = InvokeId;
			InvokeId = default;

			if (tmpInvokeId is not null)
			{
				if (await InvokeControllerFactory().ConfigureAwait(false) is { } invokeController)
				{
					await invokeController.Cancel(tmpInvokeId).ConfigureAwait(false);
				}
			}
		}
	}
}