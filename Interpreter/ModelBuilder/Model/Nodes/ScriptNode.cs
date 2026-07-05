// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Ancestor;
using Xtate.StateMachine;

namespace Xtate.Interpreter.Model;

[InstantiatedByIoC]
public class ScriptNode(DocumentIdNode documentIdNode, IScript script) : ExecutableEntityNode(documentIdNode, script), IScript, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => script;

#endregion

#region Interface IScript

	public IScriptExpression? Content => script.Content;

	public IExternalScriptExpression? Source => script.Source;

#endregion
}