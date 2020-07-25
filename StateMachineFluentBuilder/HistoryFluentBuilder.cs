using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Xtate.Annotations;

namespace Xtate.Builder
{
	[PublicAPI]
	public class HistoryFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
	{
		private readonly IHistoryBuilder  _builder;
		private readonly Action<IHistory> _builtAction;
		private readonly IBuilderFactory  _factory;
		private readonly TOuterBuilder    _outerBuilder;

		public HistoryFluentBuilder(IBuilderFactory factory, TOuterBuilder outerBuilder, Action<IHistory> builtAction)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_builder = factory.CreateHistoryBuilder(null);
			_outerBuilder = outerBuilder;
			_builtAction = builtAction;
		}

		[return: NotNull]
		public TOuterBuilder EndHistory()
		{
			_builtAction(_builder.Build());

			return _outerBuilder;
		}

		public HistoryFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

		public HistoryFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));

			_builder.SetId(id);

			return this;
		}

		public HistoryFluentBuilder<TOuterBuilder> SetType(HistoryType type)
		{
			if (type < HistoryType.Shallow || type > HistoryType.Deep) throw new InvalidEnumArgumentException(nameof(type), (int) type, typeof(HistoryType));

			_builder.SetType(type);

			return this;
		}

		public TransitionFluentBuilder<HistoryFluentBuilder<TOuterBuilder>> BeginTransition() =>
				new TransitionFluentBuilder<HistoryFluentBuilder<TOuterBuilder>>(_factory, this, _builder.SetTransition);

		public HistoryFluentBuilder<TOuterBuilder> AddTransition(string target) => AddTransition((Identifier) target);

		public HistoryFluentBuilder<TOuterBuilder> AddTransition(IIdentifier target) => BeginTransition().SetTarget(target).EndTransition();
	}
}