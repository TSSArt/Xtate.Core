using System.IO;
using System.Runtime.CompilerServices;
using Xtate.Annotations;

#if !NETSTANDARD2_1
using System.Threading.Tasks;

#endif

namespace Xtate
{
	[PublicAPI]
	public static class StreamExtensions
	{
		public static ConfiguredStreamAwaitable ConfigureAwait(this Stream stream, bool continueOnCapturedContext) => new ConfiguredStreamAwaitable(stream, continueOnCapturedContext);
	}

	[PublicAPI]
	public readonly struct ConfiguredStreamAwaitable
	{
		private readonly Stream _stream;
		private readonly bool   _continueOnCapturedContext;

		public ConfiguredStreamAwaitable(Stream stream, bool continueOnCapturedContext)
		{
			_stream = stream;
			_continueOnCapturedContext = continueOnCapturedContext;
		}

#if NETSTANDARD2_1
		public ConfiguredValueTaskAwaitable DisposeAsync() => _stream.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
#else
		public ConfiguredValueTaskAwaitable DisposeAsync()
		{
			_stream.Dispose();

			return new ValueTask().ConfigureAwait(_continueOnCapturedContext);
		}
#endif
	}
}