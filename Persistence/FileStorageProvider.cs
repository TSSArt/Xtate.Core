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
using System.IO;
using System.Text;

namespace Xtate.Persistence;

public class FileStorageProvider : IStorageProvider
{
	private static readonly char[]   InvalidFileNameChars   = Path.GetInvalidFileNameChars();
	private static readonly string[] InvalidCharReplacement = GetInvalidCharReplacement();

	private readonly string? _extension;
	private readonly string  _path;

	public FileStorageProvider(string path, string? extension = default)
	{
		Infra.Requires(path);

		_path = path;
		_extension = extension;
	}

	public required Func<Stream, ValueTask<ITransactionalStorage>> TransactionalStorageFactory { private get; [UsedImplicitly] init; }

#region Interface IStorageProvider

	public async ValueTask<ITransactionalStorage> GetTransactionalStorage(string? partition, string key)
	{
		Infra.RequiresNonEmptyString(key);

		var dir = !Infra.IsNullOrEmpty(partition) ? Path.Combine(_path, Escape(partition)) : _path;

		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}

		var path = Path.Combine(dir, Escape(key) + _extension);
		var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, FileOptions.Asynchronous);
		return await TransactionalStorageFactory(fileStream).ConfigureAwait(false);
	}

	public ValueTask RemoveTransactionalStorage(string? partition, string key)
	{
		Infra.RequiresNonEmptyString(key);

		var dir = !Infra.IsNullOrEmpty(partition) ? Path.Combine(_path, Escape(partition)) : _path;
		var path = Path.Combine(dir, Escape(key) + _extension);

		try
		{
			File.Delete(path);
		}
		catch (FileNotFoundException)
		{
			// Ignore
		}

		return default;
	}

	public ValueTask RemoveAllTransactionalStorage(string? partition)
	{
		var path = !Infra.IsNullOrEmpty(partition) ? Path.Combine(_path, Escape(partition)) : _path;

		try
		{
			Directory.Delete(path, recursive: true);
		}
		catch (DirectoryNotFoundException)
		{
			// Ignore
		}

		return default;
	}

#endregion

	private static string[] GetInvalidCharReplacement()
	{
		var list = new string[InvalidFileNameChars.Length];

		for (var i = 0; i < list.Length; i ++)
		{
			list[i] = @"_x" + ((int) InvalidFileNameChars[i]).ToString(format: @"X", CultureInfo.InvariantCulture);
		}

		return list;
	}

	private static string Escape(string name)
	{
		if (name.IndexOfAny(InvalidFileNameChars) < 0)
		{
			return name;
		}

		var sb = new StringBuilder();
		foreach (var ch in name)
		{
			var index = Array.IndexOf(InvalidFileNameChars, ch);
			if (index >= 0)
			{
				sb.Append(InvalidCharReplacement[index]);
			}
			else
			{
				sb.Append(ch);
			}
		}

		return sb.ToString();
	}
}