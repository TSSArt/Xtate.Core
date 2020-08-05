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

namespace Xtate
{
	public struct ContentEntity : IContent, IVisitorEntity<ContentEntity, IContent>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IContent

		public IValueExpression? Expression { get; set; }
		public IContentBody?     Body       { get; set; }

	#endregion

	#region Interface IVisitorEntity<ContentEntity,IContent>

		void IVisitorEntity<ContentEntity, IContent>.Init(IContent source)
		{
			Ancestor = source;
			Expression = source.Expression;
			Body = source.Body;
		}

		bool IVisitorEntity<ContentEntity, IContent>.RefEquals(ref ContentEntity other) =>
				ReferenceEquals(Expression, other.Expression) &&
				ReferenceEquals(Body, other.Body);

	#endregion
	}
}