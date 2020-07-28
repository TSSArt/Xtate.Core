#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;

namespace Xtate
{
	public struct DataEntity : IData, IVisitorEntity<DataEntity, IData>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IData

		public string?                  Id            { get; set; }
		public IExternalDataExpression? Source        { get; set; }
		public IValueExpression?        Expression    { get; set; }
		public IInlineContent?          InlineContent { get; set; }

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IVisitorEntity<DataEntity,IData>

		void IVisitorEntity<DataEntity, IData>.Init(IData source)
		{
			Ancestor = source;
			Id = source.Id;
			Source = source.Source;
			Expression = source.Expression;
			InlineContent = source.InlineContent;
		}

		bool IVisitorEntity<DataEntity, IData>.RefEquals(ref DataEntity other) =>
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(Source, other.Source) &&
				ReferenceEquals(InlineContent, other.InlineContent) &&
				ReferenceEquals(Expression, other.Expression);

	#endregion
	}
}