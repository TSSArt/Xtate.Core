using System;

namespace Xtate
{
	internal static class IdGenerator
	{
		public static string NewSendId(int hash) => NewGuidWithHash(hash);

		public static string NewSessionId(int hash) => NewGuidWithHash(hash);

		public static string NewInvokeUniqueId(int hash) => NewGuidWithHash(hash);

		public static string NewId(int hash) => NewGuidWithHash(hash);

#if NETSTANDARD2_1
		public static string NewInvokeId(string id, int hash) =>
				string.Create(46 + id.Length, (id, hash), (span, arg) =>
														  {
															  arg.id.AsSpan().CopyTo(span);
															  span[arg.id.Length] = '.';
															  span = span.Slice(arg.id.Length + 1);
															  Guid.NewGuid().TryFormat(span, out var pos, format: "D");
															  span[pos] = '-';
															  hash.TryFormat(span.Slice(pos + 1), out pos, format: "x8");
														  });

		private static string NewGuidWithHash(int hash) =>
				string.Create(length: 45, hash, (span, h) =>
												{
													Guid.NewGuid().TryFormat(span, out var pos, format: "D");
													span[pos] = '-';
													hash.TryFormat(span.Slice(pos + 1), out pos, format: "x8");
												});
#else
		public static string NewInvokeId(string id, int hash) => id + "." + Guid.NewGuid().ToString("D") + "-" + hash.ToString("x8");

		private static string NewGuidWithHash(int hash) => Guid.NewGuid().ToString("D") + "-" + hash.ToString("x8");
#endif
	}
}