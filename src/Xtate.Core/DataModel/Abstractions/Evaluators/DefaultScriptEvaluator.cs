#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IScript

	public IScriptExpression?         Content => _script.Content;
	public IExternalScriptExpression? Source  => _script.Source;

#endregion
}

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
}