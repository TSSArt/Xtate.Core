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

namespace Xtate.IoProcessors.Http;

[InstantiatedByIoC]
public class HttpIoProcessorOptions
{
	public string? ListenUrl
	{
		get;
		set
		{
			Infra.NotNull(value);

			field = value;
		}
	}

	public string PublicBaseUrl
	{
		get;
		set
		{
			Infra.NotNull(value);

			field = value;
		}
	} = @"http://notexist.invalid/";

	public long MaxMessageSize
	{
		get;
		set
		{
			Infra.RequiresNonNegative(value);

			field = value;
		}
	} = 0;

	public TimeSpan Timeout
	{
		get;
		set
		{
			if (value < System.Threading.Timeout.InfiniteTimeSpan)
			{
				throw new ArgumentException($"'{value}' is not a valid timeout value.", nameof(value));
			}

			field = value;
		}
	} = System.Threading.Timeout.InfiniteTimeSpan;
}