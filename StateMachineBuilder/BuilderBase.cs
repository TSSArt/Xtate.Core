using System;

namespace Xtate.Builder
{
	public abstract class BuilderBase
	{
		private readonly IErrorProcessor _errorProcessor;

		protected BuilderBase(IErrorProcessor errorProcessor, object? ancestor)
		{
			_errorProcessor = errorProcessor;
			Ancestor = ancestor;
		}

		protected object? Ancestor { get; }

		protected void AddError(string message, Exception? exception = default)
		{
			_errorProcessor.AddError(GetType(), Ancestor, message, exception);
		}
	}
}