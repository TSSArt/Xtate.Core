// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Persistence;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;

namespace Xtate.IoProcessors.NamedPipe;

public readonly struct NamedPipeResponseMessage : IStoreSupport
{
	public readonly NamedPipeErrorType ErrorType;

	public readonly string? ExceptionMessage;

	public readonly string? ExceptionText;

	public NamedPipeResponseMessage(DateTime timestamp) => Timestamp = timestamp;

	public NamedPipeResponseMessage(DateTime? timestamp, Exception exception)
	{
		Timestamp = timestamp;
		ErrorType = NamedPipeErrorType.Exception;
		ExceptionMessage = exception.Message;

#if DEBUG

		ExceptionText = exception.ToString();

#endif
	}

	public NamedPipeResponseMessage(in Bucket bucket)
	{
		if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo.Message)
		{
			throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
		}

		Timestamp = bucket.TryGet(Key.Id, out DateTime timestamp) ? timestamp : null;
		ErrorType = bucket.TryGet(Key.Type, out NamedPipeErrorType errorType) ? errorType : NamedPipeErrorType.None;
		ExceptionMessage = bucket.TryGet(Key.Message, out string? message) ? message : null;
		ExceptionText = bucket.TryGet(Key.Exception, out string? text) ? text : null;
	}

	public DateTime? Timestamp { get; }

#region Interface IStoreSupport

	public void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.Message);

		if (Timestamp is not null)
		{
			bucket.Add(Key.Id, Timestamp.Value);
		}

		if (ErrorType != NamedPipeErrorType.None)
		{
			bucket.Add(Key.Type, ErrorType);
			bucket.Add(Key.Message, ExceptionMessage);
			bucket.Add(Key.Exception, ExceptionText);
		}
	}

#endregion
}