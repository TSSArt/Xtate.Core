// Copyright © 2019-2025 Sergii Artemenko
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

using System.Text;

namespace Xtate.Core;

[InterpolatedStringHandler]
public readonly struct LoggingInterpolatedStringHandler
{
    private readonly ImmutableArray<LoggingParameter>.Builder? _parametersBuilder;

    private readonly IFormatProvider? _provider;

    private readonly StringBuilder? _stringBuilder;

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

    public string? ToString(out ImmutableArray<LoggingParameter> parameters)
    {
        parameters = _parametersBuilder?.MoveToImmutable() ?? default;
        var result = _stringBuilder?.ToString();

        return result;
    }

    public void AppendLiteral(string value) => _stringBuilder!.Append(value);

    public void AppendFormatted(object? value, string? format = default, [CallerArgumentExpression(nameof(value))] string? expression = default)
    {
        Span<char> buf = stackalloc char[StackSpan<char>.MaxLengthInStack];

        if (value is ISpanFormattable spanFormattable && spanFormattable.TryFormat(buf, out var charsWritten, format.AsSpan(), _provider))
        {
            _stringBuilder!.Append(buf[..charsWritten]);
        }
        else if (value is IFormattable formattable)
        {
            _stringBuilder!.Append(formattable.ToString(format, _provider));
        }
        else
        {
            _stringBuilder!.Append(value);
        }

        _parametersBuilder!.Add(new LoggingParameter(expression!, value, format));
    }

    public void AppendFormatted(object? value,
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
}