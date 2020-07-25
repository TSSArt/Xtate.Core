using System;
using Xtate.Annotations;

namespace Xtate
{
	public enum StateMachineOriginType
	{
		None,
		Scxml,
		Source,
		StateMachine
	}

	[PublicAPI]
	public readonly struct StateMachineOrigin
	{
		private readonly object? _val;

		public StateMachineOrigin(IStateMachine stateMachine, Uri? baseUri = null)
		{
			_val = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
			BaseUri = baseUri;
		}

		public StateMachineOrigin(Uri source, Uri? baseUri = null)
		{
			_val = source ?? throw new ArgumentNullException(nameof(source));
			BaseUri = baseUri;
		}

		public StateMachineOrigin(string scxml, Uri? baseUri = null)
		{
			_val = scxml ?? throw new ArgumentNullException(nameof(scxml));
			BaseUri = baseUri;
		}

		public StateMachineOriginType Type =>
				_val switch
				{
						string _ => StateMachineOriginType.Scxml,
						Uri _ => StateMachineOriginType.Source,
						IStateMachine _ => StateMachineOriginType.StateMachine,
						null => StateMachineOriginType.None,
						_ => Infrastructure.UnexpectedValue<StateMachineOriginType>()
				};

		public Uri? BaseUri { get; }

		public string AsScxml() => (string?) _val ?? throw new ArgumentException(Resources.Exception_Value_is_not_SCXML);

		public Uri AsSource() => (Uri?) _val ?? throw new ArgumentException(Resources.Exception_Value_is_not_Source);

		public IStateMachine AsStateMachine() => (IStateMachine?) _val ?? throw new ArgumentException(Resources.Exception_Value_is_not_StateMachine);
	}
}