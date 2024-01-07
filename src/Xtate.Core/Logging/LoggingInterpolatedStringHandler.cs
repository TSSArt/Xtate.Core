using System.Globalization;
using System.Text;

namespace Xtate.Core;

[InterpolatedStringHandler]
public readonly struct LoggingInterpolatedStringHandler
{
	private readonly ImmutableArray<LoggingParameter>.Builder? _parametersBuilder;
	private readonly IFormatProvider?                          _provider;
	private readonly StringBuilder?                            _stringBuilder;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public LoggingInterpolatedStringHandler(int literalLength,
											int formattedCount,
											ILogger logger,
											Level level,
											out bool shouldFormat)
	{
		if (logger.IsEnabled(level))
		{
			if (formattedCount > 0)
			{
				_provider = logger.FormatProvider;
				_parametersBuilder = ImmutableArray.CreateBuilder<LoggingParameter>(formattedCount);
			}

			_stringBuilder = new StringBuilder(literalLength + formattedCount * 16);
			shouldFormat = true;
		}
		else
		{
			shouldFormat = false;
		}
	}

	public string ToString(out ImmutableArray<LoggingParameter> parameters)
	{
		parameters = _parametersBuilder?.MoveToImmutable() ?? default;
		var result = _stringBuilder!.ToString();

		return result;
	}

	public void AppendLiteral(string value) => _stringBuilder!.Append(value);

	[SuppressMessage("Style", "IDE0038:Use pattern matching", Justification = "Avoid boxing if T is struct")]
	[SuppressMessage("ReSharper", "MergeCastWithTypeCheck", Justification = "Avoid boxing if T is struct")]
	private string? ToStringFormatted<T>(T value, string? format)
	{
		if (_provider is not null && _provider.GetType() != typeof(CultureInfo) && _provider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter customFormatter)
		{
			customFormatter.Format(format, value, _provider);
		}

		if (value is IFormattable)
		{
			return ((IFormattable) value).ToString(format, _provider);
		}

		return value is not null ? value.ToString() : default;
	}

	public void AppendFormatted<T>(T value, string? format = default, [CallerArgumentExpression(nameof(value))] string? expression = default)
	{
		if (ToStringFormatted(value, format) is { } str)
		{
			_stringBuilder!.Append(str);
		}

		_parametersBuilder!.Add(new LoggingParameter(expression!, value, format));
	}

	public void AppendFormatted<T>(T value,
								   int alignment,
								   string? format = default,
								   [CallerArgumentExpression(nameof(value))]
								   string? expression = default)
	{
		var start = _stringBuilder!.Length;

		AppendFormatted(value, format, expression);

		if (Math.Abs(alignment) - (_stringBuilder.Length - start) is var paddingRequired and > 0)
		{
			if (alignment < 0)
			{
				_stringBuilder.Append(value: ' ', paddingRequired);
			}
			else
			{
				_stringBuilder.Insert(start, value: @" ", paddingRequired);
			}
		}
	}

	public void AppendFormatted(object? value,
								int alignment = 0,
								string? format = null,
								[CallerArgumentExpression(nameof(value))]
								string? expression = default) =>
		AppendFormatted<object?>(value, alignment, format, expression);
}