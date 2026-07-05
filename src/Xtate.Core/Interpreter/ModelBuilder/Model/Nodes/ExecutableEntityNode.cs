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

using Xtate.Ancestor.Extensions;
using Xtate.DataModel;
using Xtate.Interpreter.Internal;
using Xtate.StateMachine;

namespace Xtate.Interpreter.Model;

public abstract class ExecutableEntityNode : IExecutableEntity, IExecEvaluator, IDocumentId
{
	private readonly IExecEvaluator _execEvaluator;

	private DocumentIdSlot _documentIdSlot;

	protected ExecutableEntityNode(DocumentIdNode documentIdNode, IExecutableEntity entity)
	{
		_execEvaluator = entity.UseAncestor.As<IExecEvaluator>();
		documentIdNode.SaveToSlot(out _documentIdSlot);
	}

#region Interface IDocumentId

	public int DocumentId => _documentIdSlot.CreateValue();

#endregion

#region Interface IExecEvaluator

	public ValueTask Execute() => _execEvaluator.Execute();

#endregion
}