using System;

namespace Xtate
{
	public interface IExternalScriptExpression : IExecutableEntity
	{
		Uri? Uri { get; }
	}
}