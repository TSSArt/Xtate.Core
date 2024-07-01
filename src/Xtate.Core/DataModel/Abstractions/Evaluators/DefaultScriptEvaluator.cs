<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2024 Sergii Artemenko
// 
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
#endregion

using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class ScriptEvaluator : IScript, IExecEvaluator, IAncestorProvider
{
	private readonly IScript _script;

	protected ScriptEvaluator(IScript script)
	{
		Infra.Requires(script);

		_script = script;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _script;
=======
namespace Xtate.DataModel;

public abstract class ScriptEvaluator(IScript script) : IScript, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => script;
>>>>>>> Stashed changes

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IScript

<<<<<<< Updated upstream
	public IScriptExpression?         Content => _script.Content;
	public IExternalScriptExpression? Source  => _script.Source;
=======
	public virtual IScriptExpression?         Content => script.Content;
	public virtual IExternalScriptExpression? Source  => script.Source;
>>>>>>> Stashed changes

#endregion
}

<<<<<<< Updated upstream
[PublicAPI]
public class DefaultScriptEvaluator : ScriptEvaluator
{
	public DefaultScriptEvaluator(IScript script) : base(script)
	{
		Infra.Assert(script.Content is not null || script.Source is not null);

		ContentEvaluator = script.Content?.As<IExecEvaluator>();
		SourceEvaluator = script.Source?.As<IExecEvaluator>();
	}

	public IExecEvaluator? ContentEvaluator { get; }
	public IExecEvaluator? SourceEvaluator  { get; }

	public override ValueTask Execute()
	{
		var evaluator = ContentEvaluator ?? SourceEvaluator;

		return evaluator!.Execute();
	}
=======
public class DefaultScriptEvaluator : ScriptEvaluator
{
	private readonly IExecEvaluator _evaluator;

	public DefaultScriptEvaluator(IScript script) : base(script)
	{
		var evaluator = base.Content?.As<IExecEvaluator>() ?? base.Source?.As<IExecEvaluator>();
		Infra.NotNull(evaluator);

		_evaluator = evaluator;
	}

	public override ValueTask Execute() => _evaluator.Execute();
>>>>>>> Stashed changes
}