using System;
using System.Xml;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public static class ErrorProcessorExtensions
	{
		public static void AddError<T>(this IErrorProcessor errorProcessor, object? entity, string message, Exception? exception = null) =>
				AddError(errorProcessor, typeof(T), entity, message, exception);

		public static void AddError(this IErrorProcessor errorProcessor, Type source, object? entity, string message, Exception? exception = null)
		{
			if (errorProcessor == null) throw new ArgumentNullException(nameof(errorProcessor));
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (message == null) throw new ArgumentNullException(nameof(message));

			if (errorProcessor.LineInfoRequired)
			{
				if (entity != null && entity.Is<IXmlLineInfo>(out var xmlLineInfo) && xmlLineInfo.HasLineInfo())
				{
					errorProcessor.AddError(new ErrorItem(source, message, exception, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition));

					return;
				}

				if (exception is XmlException xmlException && xmlException.LineNumber > 0)
				{
					errorProcessor.AddError(new ErrorItem(source, message, exception, xmlException.LineNumber, xmlException.LinePosition));

					return;
				}
			}

			errorProcessor.AddError(new ErrorItem(source, message, exception));
		}
	}
}