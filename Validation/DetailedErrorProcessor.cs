using System;
using System.Collections.Immutable;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public sealed class DetailedErrorProcessor : IErrorProcessor
	{
		private readonly StateMachineOrigin _origin;
		private readonly SessionId?         _sessionId;

		private ImmutableArray<ErrorItem>.Builder? _errors;

		public DetailedErrorProcessor(SessionId? sessionId, StateMachineOrigin origin)
		{
			_sessionId = sessionId;
			_origin = origin;
		}

	#region Interface IErrorProcessor

		public void ThrowIfErrors()
		{
			var errors = _errors;
			_errors = null;

			if (errors != null)
			{
				throw new StateMachineValidationException(errors.ToImmutable(), _sessionId, _origin);
			}
		}

		void IErrorProcessor.AddError(ErrorItem errorItem)
		{
			if (errorItem == null) throw new ArgumentNullException(nameof(errorItem));

			(_errors ??= ImmutableArray.CreateBuilder<ErrorItem>()).Add(errorItem);
		}

		bool IErrorProcessor.LineInfoRequired => true;

	#endregion
	}
}