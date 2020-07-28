#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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

using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ContentNode : IContent, IStoreSupport, IAncestorProvider
	{
		private readonly ContentEntity _content;

		public ContentNode(in ContentEntity content) => _content = content;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _content.Ancestor;

	#endregion

	#region Interface IContent

		public IValueExpression? Expression => _content.Expression;

		public IContentBody? Body => _content.Body;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ContentNode);
			bucket.AddEntity(Key.Expression, Expression);
			bucket.Add(Key.Body, Body?.Value);
		}

	#endregion
	}
}