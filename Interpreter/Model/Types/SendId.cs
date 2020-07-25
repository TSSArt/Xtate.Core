using System;
using System.Diagnostics.CodeAnalysis;

namespace Xtate
{
	[Serializable]
	public sealed class SendId : LazyId
	{
		private SendId() { }

		private SendId(string val) : base(val) { }

		protected override string GenerateId() => IdGenerator.NewSendId(GetHashCode());

		public static SendId New() => new SendId();

		[return: NotNullIfNotNull("val")]
		public static SendId? FromString(string? val) => val != null ? new SendId(val) : null;
	}
}