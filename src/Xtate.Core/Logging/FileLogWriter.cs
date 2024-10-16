﻿// Copyright © 2019-2024 Sergii Artemenko
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

using System.Diagnostics;

namespace Xtate.Core;

//TODO:delete
[Obsolete]
public class FileLogWriter<TSource> : TraceLogWriter<TSource>
{
	public FileLogWriter(string file) : base(null)
	{
		var listenerCollection = Trace.Listeners;

		if (listenerCollection.OfType<FileListener>().All(listener => listener.FileName != file))
		{
			listenerCollection.Add(new FileListener(file));
		}
	}

	private class FileListener(string fileName) : TextWriterTraceListener(fileName)
	{
		public string FileName { get; } = fileName;
	}
}