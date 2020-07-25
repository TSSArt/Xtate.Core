using System;

namespace Xtate.Builder
{
	public class ScriptBuilder : BuilderBase, IScriptBuilder
	{
		private IScriptExpression?         _body;
		private IExternalScriptExpression? _source;

		public ScriptBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IScriptBuilder

		public IScript Build() => new ScriptEntity { Ancestor = Ancestor, Source = _source, Content = _body };

		public void SetSource(IExternalScriptExpression source) => _source = source ?? throw new ArgumentNullException(nameof(source));

		public void SetBody(IScriptExpression body) => _body = body ?? throw new ArgumentNullException(nameof(body));

	#endregion
	}
}