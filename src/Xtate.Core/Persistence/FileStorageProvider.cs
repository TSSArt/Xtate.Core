#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Persistence
{
	[PublicAPI]
	public class FileStorageProvider : IStorageProvider
	{
		private static readonly char[]   InvalidFileNameChars   = Path.GetInvalidFileNameChars();
		private static readonly string[] InvalidCharReplacement = GetInvalidCharReplacement();

		private readonly string? _extension;
		private readonly string  _path;

		public FileStorageProvider(string path, string? extension = default)
		{
			_path = path ?? throw new ArgumentNullException(nameof(path));
			_extension = extension;
		}

	#region Interface IStorageProvider

		public async ValueTask<ITransactionalStorage> GetTransactionalStorage(string? partition, string key, CancellationToken token)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(key));

			var dir = !string.IsNullOrEmpty(partition) ? Path.Combine(_path, Escape(partition)) : _path;

			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(_path);
			}

			var path = Path.Combine(dir, Escape(key) + _extension);
			var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, FileOptions.Asynchronous);
			var streamStorage = await StreamStorage.CreateAsync(fileStream, disposeStream: true, token).ConfigureAwait(false);

			return streamStorage;
		}

		public ValueTask RemoveTransactionalStorage(string? partition, string key, CancellationToken token)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(key));

			var dir = !string.IsNullOrEmpty(partition) ? Path.Combine(_path, Escape(partition)) : _path;
			var path = Path.Combine(dir, Escape(key) + _extension);

			try
			{
				File.Delete(path);
			}
			catch (IOException) { }

			return default;
		}

		public ValueTask RemoveAllTransactionalStorage(string? partition, CancellationToken token)
		{
			var path = !string.IsNullOrEmpty(partition) ? Path.Combine(_path, Escape(partition)) : _path;

			try
			{
				Directory.Delete(path, recursive: true);
			}
			catch (IOException) { }

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
}