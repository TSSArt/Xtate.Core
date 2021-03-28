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
using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class SendNode : ExecutableEntityNode, ISend, IAncestorProvider, IDebugEntityId
	{
		private readonly ISend _send;

		public SendNode(DocumentIdNode documentIdNode, ISend send) : base(documentIdNode, send) => _send = send;

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _send;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

	#endregion

	#region Interface ISend

		public string?                             EventName        => _send.EventName;
		public IValueExpression?                   EventExpression  => _send.EventExpression;
		public Uri?                                Target           => _send.Target;
		public IValueExpression?                   TargetExpression => _send.TargetExpression;
		public Uri?                                Type             => _send.Type;
		public IValueExpression?                   TypeExpression   => _send.TypeExpression;
		public string?                             Id               => _send.Id;
		public ILocationExpression?                IdLocation       => _send.IdLocation;
		public int?                                DelayMs          => _send.DelayMs;
		public IValueExpression?                   DelayExpression  => _send.DelayExpression;
		public ImmutableArray<ILocationExpression> NameList         => _send.NameList;
		public ImmutableArray<IParam>              Parameters       => _send.Parameters;
		public IContent?                           Content          => _send.Content;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.SendNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.Add(Key.Id, Id);
			bucket.Add(Key.Type, Type);
			bucket.Add(Key.Event, EventName);
			bucket.Add(Key.Target, Target);
			bucket.Add(Key.DelayMs, DelayMs ?? 0);
			bucket.AddEntity(Key.TypeExpression, TypeExpression);
			bucket.AddEntity(Key.EventExpression, EventExpression);
			bucket.AddEntity(Key.TargetExpression, TargetExpression);
			bucket.AddEntity(Key.DelayExpression, DelayExpression);
			bucket.AddEntity(Key.IdLocation, IdLocation);
			bucket.AddEntityList(Key.NameList, NameList);
			bucket.AddEntityList(Key.Parameters, Parameters);
			bucket.AddEntity(Key.Content, Content);
		}
	}
}