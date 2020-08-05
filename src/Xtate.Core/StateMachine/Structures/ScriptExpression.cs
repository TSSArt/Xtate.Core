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
	public struct ScriptExpression : IScriptExpression, IVisitorEntity<ScriptExpression, IScriptExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IScriptExpression

		public string? Expression { get; set; }

	#endregion

	#region Interface IVisitorEntity<ScriptExpression,IScriptExpression>

		void IVisitorEntity<ScriptExpression, IScriptExpression>.Init(IScriptExpression source)
		{
			Ancestor = source;
			Expression = source.Expression;
		}

		bool IVisitorEntity<ScriptExpression, IScriptExpression>.RefEquals(ref ScriptExpression other) => ReferenceEquals(Expression, other.Expression);

	#endregion
	}
}