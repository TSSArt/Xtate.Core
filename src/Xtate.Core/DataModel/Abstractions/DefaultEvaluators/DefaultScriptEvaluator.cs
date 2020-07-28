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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultScriptEvaluator : IScript, IExecEvaluator, IAncestorProvider
	{
		private readonly ScriptEntity _script;

		public DefaultScriptEvaluator(in ScriptEntity script)
		{
			_script = script;

			Infrastructure.Assert(script.Content != null || script.Source != null);

			ContentEvaluator = script.Content?.As<IExecEvaluator>();
			SourceEvaluator = script.Source?.As<IExecEvaluator>();
		}

		public IExecEvaluator? ContentEvaluator { get; }
		public IExecEvaluator? SourceEvaluator  { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _script.Ancestor;

	#endregion

	#region Interface IExecEvaluator

		public virtual ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var evaluator = ContentEvaluator ?? SourceEvaluator;
			return evaluator!.Execute(executionContext, token);
		}

	#endregion

	#region Interface IScript

		public IScriptExpression? Content => _script.Content;

		public IExternalScriptExpression? Source => _script.Source;

	#endregion
	}
}