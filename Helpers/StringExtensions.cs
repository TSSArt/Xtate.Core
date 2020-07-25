using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Xtate
{
	public static class StringExtensions
	{
		/// <summary>
		///     Returns string where leading and trailing whitespace characters are removed and sequence of whitespace characters
		///     replaced to single space character.
		///     If source string does not expect any normalization then instance of original string will be returned.
		/// </summary>
		/// <param name="str">String to normalize whitespaces</param>
		/// <returns>Normalized string</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[SuppressMessage(category: "ReSharper", checkId: "SuggestVarOrType_Elsewhere")]
		public static string NormalizeSpaces(this string str)
		{
			if (str == null) throw new ArgumentNullException(nameof(str));

			if (str.Length == 0) return string.Empty;

			if (str.Length <= 32768)
			{
				Span<char> buf = stackalloc char[str.Length];

				return RemoveSpaces(str, buf);
			}

			var array = ArrayPool<char>.Shared.Rent(str.Length);
			try
			{
				return RemoveSpaces(str, array);
			}
			finally
			{
				ArrayPool<char>.Shared.Return(array);
			}
		}

		private static string RemoveSpaces(string str, Span<char> buf)
		{
			var isInWhiteSpace = true;
			var addSpace = false;
			var normalized = false;
			var count = 0;

			foreach (var ch in str)
			{
				if (char.IsWhiteSpace(ch))
				{
					if (ch != ' ')
					{
						normalized = true;
					}

					if (!isInWhiteSpace)
					{
						isInWhiteSpace = true;
						addSpace = true;
					}

					continue;
				}

				if (isInWhiteSpace)
				{
					isInWhiteSpace = false;

					if (addSpace)
					{
						buf[count ++] = ' ';
						addSpace = false;
					}
				}

				buf[count ++] = ch;
			}

			return str.Length == count && !normalized ? str : buf.Slice(start: 0, count).ToString();
		}
	}
}