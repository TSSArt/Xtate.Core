// Copyright © 2019-2024 Sergii Artemenko
// 
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

namespace Xtate.Builder;

public class ScriptBuilder : BuilderBase, IScriptBuilder
{
	private IScriptExpression?         _body;
	private IExternalScriptExpression? _source;

#region Interface IScriptBuilder

	public IScript Build() => new ScriptEntity { Ancestor = Ancestor, Source = _source, Content = _body };

	public void SetSource(IExternalScriptExpression source)
	{
		Infra.Requires(source);

		_source = source;
	}

	public void SetBody(IScriptExpression body)
	{
		Infra.Requires(body);

		_body = body;
	}

#endregion
}