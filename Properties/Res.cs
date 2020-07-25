using System.Diagnostics.CodeAnalysis;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	[ExcludeFromCodeCoverage]
	internal static class Res
	{
		public static string Format(string format, object arg)                            => string.Format(Resources.Culture, format, arg);
		public static string Format(string format, object arg0, object arg1)              => string.Format(Resources.Culture, format, arg0, arg1);
		public static string Format(string format, object arg0, object arg1, object arg2) => string.Format(Resources.Culture, format, arg0, arg1, arg2);
		public static string Format(string format, params object[] args)                  => string.Format(Resources.Culture, format, args);
	}
}