using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Xtate.Annotations;

namespace Xtate.Builder
{
	[PublicAPI]
	public class FinalFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
	{
		private readonly IFinalBuilder   _builder;
		private readonly Action<IFinal>  _builtAction;
		private readonly IBuilderFactory _factory;
		private readonly TOuterBuilder   _outerBuilder;

		public FinalFluentBuilder(IBuilderFactory factory, TOuterBuilder outerBuilder, Action<IFinal> builtAction)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_builder = factory.CreateFinalBuilder(null);
			_outerBuilder = outerBuilder;
			_builtAction = builtAction;
		}

		[return: NotNull]
		public TOuterBuilder EndFinal()
		{
			_builtAction(_builder.Build());

			return _outerBuilder;
		}

		public FinalFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

		public FinalFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));

			_builder.SetId(id);

			return this;
		}

		private FinalFluentBuilder<TOuterBuilder> SetDoneData(IValueExpression evaluator)
		{
			var contentBuilder = _factory.CreateContentBuilder(null);
			contentBuilder.SetExpression(evaluator);

			var doneData = _factory.CreateDoneDataBuilder(null);
			doneData.SetContent(contentBuilder.Build());

			_builder.SetDoneData(doneData.Build());

			return this;
		}

		public FinalFluentBuilder<TOuterBuilder> SetDoneData(Evaluator evaluator) => SetDoneData(new RuntimeEvaluator(evaluator));

		public FinalFluentBuilder<TOuterBuilder> SetDoneData(EvaluatorTask task) => SetDoneData(new RuntimeEvaluator(task));

		public FinalFluentBuilder<TOuterBuilder> SetDoneData(EvaluatorCancellableTask task) => SetDoneData(new RuntimeEvaluator(task));

		private FinalFluentBuilder<TOuterBuilder> AddOnEntry(RuntimeAction action)
		{
			_builder.AddOnEntry(new OnEntryEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public FinalFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableAction action) => AddOnEntry(new RuntimeAction(action));

		public FinalFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableTask task) => AddOnEntry(new RuntimeAction(task));

		public FinalFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableCancellableTask task) => AddOnEntry(new RuntimeAction(task));

		private FinalFluentBuilder<TOuterBuilder> AddOnExit(RuntimeAction action)
		{
			_builder.AddOnExit(new OnExitEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public FinalFluentBuilder<TOuterBuilder> AddOnExit(ExecutableAction action) => AddOnExit(new RuntimeAction(action));

		public FinalFluentBuilder<TOuterBuilder> AddOnExit(ExecutableTask task) => AddOnExit(new RuntimeAction(task));

		public FinalFluentBuilder<TOuterBuilder> AddOnExit(ExecutableCancellableTask task) => AddOnExit(new RuntimeAction(task));
	}
}