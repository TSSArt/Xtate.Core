// Copyright © 2019-2024 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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

	[SuppressMessage(category: "Style", checkId: "IDE0038:Use pattern matching", Justification = "Avoid boxing if T is struct")]
	[SuppressMessage(category: "ReSharper", checkId: "MergeCastWithTypeCheck", Justification = "Avoid boxing if T is struct")]
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