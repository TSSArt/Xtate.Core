namespace Xtate.Builder
{
	public class ElseBuilder : BuilderBase, IElseBuilder
	{
		public ElseBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IElseBuilder

		public IElse Build() => new ElseEntity { Ancestor = Ancestor };

	#endregion
	}
}