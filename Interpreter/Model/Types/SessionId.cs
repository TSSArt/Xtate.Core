using System;
using System.Diagnostics.CodeAnalysis;

namespace Xtate
{
	[Serializable]
	public sealed class SessionId : LazyId
	{
		private SessionId() { }

		private SessionId(string val) : base(val) { }
		protected override string GenerateId() => IdGenerator.NewSessionId(GetHashCode());

		public static SessionId New() => new SessionId();

		[return: NotNullIfNotNull("val")]
		public static SessionId? FromString(string? val) => val != null ? new SessionId(val) : null;
	}
}