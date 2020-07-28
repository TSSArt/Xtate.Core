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
	public struct ExternalDataExpression : IExternalDataExpression, IVisitorEntity<ExternalDataExpression, IExternalDataExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IExternalDataExpression

		public Uri? Uri { get; set; }

	#endregion

	#region Interface IVisitorEntity<ExternalDataExpression,IExternalDataExpression>

		void IVisitorEntity<ExternalDataExpression, IExternalDataExpression>.Init(IExternalDataExpression source)
		{
			Ancestor = source;
			Uri = source.Uri;
		}

		bool IVisitorEntity<ExternalDataExpression, IExternalDataExpression>.RefEquals(ref ExternalDataExpression other) => ReferenceEquals(Uri, other.Uri);

	#endregion
	}
}