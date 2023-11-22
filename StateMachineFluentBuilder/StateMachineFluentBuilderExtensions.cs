using System;
using Xtate.IoC;

namespace Xtate.Builder
{
	public static class StateMachineFluentBuilderExtensions
	{
		private class TOuterBuilder : IStub
		{
			public bool IsMatch(Type type) => true;
		}

		public static void RegisterStateMachineFluentBuilder(this IServiceCollection services)
		{
			services.RegisterStateMachineBuilder();

			services.AddTypeSync<StateMachineFluentBuilder>();
			services.AddTypeSync<StateFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IState>>();
			services.AddTypeSync<ParallelFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IParallel>>();
			services.AddTypeSync<FinalFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IFinal>>();
			services.AddTypeSync<InitialFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IInitial>>();
			services.AddTypeSync<HistoryFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<IHistory>>();
			services.AddTypeSync<TransitionFluentBuilder<TOuterBuilder>, TOuterBuilder, Action<ITransition>>();
		}
	}
}
