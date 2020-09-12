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

namespace Xtate
{
	public struct InvokeEntity : IInvoke, IVisitorEntity<InvokeEntity, IInvoke>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IInvoke

		public bool                                AutoForward      { get; set; }
		public IContent?                           Content          { get; set; }
		public IFinalize?                          Finalize         { get; set; }
		public string?                             Id               { get; set; }
		public ILocationExpression?                IdLocation       { get; set; }
		public ImmutableArray<ILocationExpression> NameList         { get; set; }
		public ImmutableArray<IParam>              Parameters       { get; set; }
		public Uri?                                Source           { get; set; }
		public IValueExpression?                   SourceExpression { get; set; }
		public Uri?                                Type             { get; set; }
		public IValueExpression?                   TypeExpression   { get; set; }

	#endregion

	#region Interface IVisitorEntity<InvokeEntity,IInvoke>

		void IVisitorEntity<InvokeEntity, IInvoke>.Init(IInvoke source)
		{
			Ancestor = source;
			Id = source.Id;
			IdLocation = source.IdLocation;
			Content = source.Content;
			Type = source.Type;
			TypeExpression = source.TypeExpression;
			Source = source.Source;
			SourceExpression = source.SourceExpression;
			NameList = source.NameList;
			Parameters = source.Parameters;
			Finalize = source.Finalize;
			AutoForward = source.AutoForward;
		}

		bool IVisitorEntity<InvokeEntity, IInvoke>.RefEquals(ref InvokeEntity other) =>
				AutoForward == other.AutoForward &&
				NameList == other.NameList &&
				Parameters == other.Parameters &&
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(IdLocation, other.IdLocation) &&
				ReferenceEquals(Content, other.Content) &&
				ReferenceEquals(Type, other.Type) &&
				ReferenceEquals(TypeExpression, other.TypeExpression) &&
				ReferenceEquals(Source, other.Source) &&
				ReferenceEquals(SourceExpression, other.SourceExpression) &&
				ReferenceEquals(Finalize, other.Finalize);

	#endregion
	}
}