using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class DoneDataBuilder : BuilderBase, IDoneDataBuilder
	{
		private IContent?                       _content;
		private ImmutableArray<IParam>.Builder? _parameters;

		public DoneDataBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IDoneDataBuilder

		public IDoneData Build() => new DoneDataEntity { Ancestor = Ancestor, Content = _content, Parameters = _parameters?.ToImmutable() ?? default };

		public void SetContent(IContent content) => _content = content ?? throw new ArgumentNullException(nameof(content));

		public void AddParameter(IParam parameter)
		{
			if (parameter == null) throw new ArgumentNullException(nameof(parameter));

			(_parameters ??= ImmutableArray.CreateBuilder<IParam>()).Add(parameter);
		}

	#endregion
	}
}