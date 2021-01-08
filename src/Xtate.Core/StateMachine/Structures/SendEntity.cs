#region Copyright © 2019-2020 Sergii Artemenko

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

namespace Xtate.Core
{
	public struct SendEntity : ISend, IVisitorEntity<SendEntity, ISend>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface ISend

		public IContent?                           Content          { get; set; }
		public IValueExpression?                   DelayExpression  { get; set; }
		public int?                                DelayMs          { get; set; }
		public string?                             EventName        { get; set; }
		public IValueExpression?                   EventExpression  { get; set; }
		public string?                             Id               { get; set; }
		public ILocationExpression?                IdLocation       { get; set; }
		public ImmutableArray<ILocationExpression> NameList         { get; set; }
		public ImmutableArray<IParam>              Parameters       { get; set; }
		public Uri?                                Target           { get; set; }
		public IValueExpression?                   TargetExpression { get; set; }
		public Uri?                                Type             { get; set; }
		public IValueExpression?                   TypeExpression   { get; set; }

	#endregion

	#region Interface IVisitorEntity<SendEntity,ISend>

		void IVisitorEntity<SendEntity, ISend>.Init(ISend source)
		{
			Ancestor = source;
			Id = source.Id;
			IdLocation = source.IdLocation;
			EventName = source.EventName;
			EventExpression = source.EventExpression;
			Content = source.Content;
			Target = source.Target;
			TargetExpression = source.TargetExpression;
			Type = source.Type;
			TypeExpression = source.TypeExpression;
			Parameters = source.Parameters;
			DelayMs = source.DelayMs;
			DelayExpression = source.DelayExpression;
			NameList = source.NameList;
		}

		bool IVisitorEntity<SendEntity, ISend>.RefEquals(ref SendEntity other) =>
				DelayMs == other.DelayMs &&
				Parameters == other.Parameters &&
				NameList == other.NameList &&
				ReferenceEquals(DelayExpression, other.DelayExpression) &&
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(IdLocation, other.IdLocation) &&
				ReferenceEquals(EventName, other.EventName) &&
				ReferenceEquals(EventExpression, other.EventExpression) &&
				ReferenceEquals(Target, other.Target) &&
				ReferenceEquals(TargetExpression, other.TargetExpression) &&
				ReferenceEquals(Type, other.Type) &&
				ReferenceEquals(TypeExpression, other.TypeExpression) &&
				ReferenceEquals(Content, other.Content);

	#endregion
	}
}