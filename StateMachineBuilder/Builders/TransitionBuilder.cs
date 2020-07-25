using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class TransitionBuilder : BuilderBase, ITransitionBuilder
	{
		private ImmutableArray<IExecutableEntity>.Builder? _actions;
		private IExecutableEntity?                         _condition;
		private ImmutableArray<IEventDescriptor>           _eventDescriptors;
		private ImmutableArray<IIdentifier>                _target;
		private TransitionType                             _type;

		public TransitionBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface ITransitionBuilder

		public ITransition Build() =>
				new TransitionEntity { Ancestor = Ancestor, EventDescriptors = _eventDescriptors, Condition = _condition, Target = _target, Type = _type, Action = _actions?.ToImmutable() ?? default };

		public void SetCondition(IExecutableEntity condition) => _condition = condition;

		public void SetTarget(ImmutableArray<IIdentifier> target) => _target = target;

		public void SetType(TransitionType type) => _type = type;

		public void SetEvent(ImmutableArray<IEventDescriptor> eventDescriptors) => _eventDescriptors = eventDescriptors;

		public void AddAction(IExecutableEntity action) => (_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);

	#endregion
	}
}