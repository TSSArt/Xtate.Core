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

namespace Xtate.Core
{
	public struct ExternalScriptExpression : IExternalScriptExpression, IVisitorEntity<ExternalScriptExpression, IExternalScriptExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri { get; set; }

	#endregion

	#region Interface IVisitorEntity<ExternalScriptExpression,IExternalScriptExpression>

		void IVisitorEntity<ExternalScriptExpression, IExternalScriptExpression>.Init(IExternalScriptExpression source)
		{
			Ancestor = source;
			Uri = source.Uri;
		}

		bool IVisitorEntity<ExternalScriptExpression, IExternalScriptExpression>.RefEquals(ref ExternalScriptExpression other) => ReferenceEquals(Uri, other.Uri);

	#endregion
	}
}