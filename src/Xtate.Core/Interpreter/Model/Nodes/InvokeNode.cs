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
	internal sealed class InvokeNode : IInvoke, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly ICancelInvokeEvaluator _cancelInvokeEvaluator;
		private readonly IInvoke                _invoke;
		private readonly IStartInvokeEvaluator  _startInvokeEvaluator;
		private          DocumentIdSlot         _documentIdSlot;
		private          IIdentifier?           _stateId;

		public InvokeNode(DocumentIdNode documentIdNode, IInvoke invoke)
		{
			documentIdNode.SaveToSlot(out _documentIdSlot);
			_invoke = invoke;

			Finalize = invoke.Finalize?.As<FinalizeNode>();

			var startInvokeEvaluator = invoke.As<IStartInvokeEvaluator>();
			Infra.NotNull(startInvokeEvaluator);
			_startInvokeEvaluator = startInvokeEvaluator;

			var cancelInvokeEvaluator = invoke.As<ICancelInvokeEvaluator>();
			Infra.NotNull(cancelInvokeEvaluator);
			_cancelInvokeEvaluator = cancelInvokeEvaluator;
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

		public void SetStateId(IIdentifier stateId) => _stateId = stateId;

		public async ValueTask Start(IExecutionContext executionContext, CancellationToken token)
		{
			Infra.NotNull(_stateId, Resources.Exception_StateIdNotInitialized);

			InvokeId = await _startInvokeEvaluator.Start(_stateId, executionContext, token).ConfigureAwait(false);
		}

		public async ValueTask Cancel(IExecutionContext executionContext, CancellationToken token)
		{
			var tmpInvokeId = InvokeId;
			InvokeId = default;

			if (tmpInvokeId is not null)
			{
				await _cancelInvokeEvaluator.Cancel(tmpInvokeId, executionContext, token).ConfigureAwait(false);
			}
		}
	}
}