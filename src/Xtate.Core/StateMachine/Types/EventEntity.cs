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

namespace Xtate.Core;

public struct EventEntity(string? value) : IOutgoingEvent
{
	public static readonly FullUri InternalTarget = new(@"_internal");

	public static readonly FullUri ParentTarget = new(@"_parent");

	public string? RawData { get; set; }

#region Interface IOutgoingEvent

	public DataModelValue Data { get; set; }

	public int DelayMs { get; set; }

	public EventName Name { get; set; } = (EventName) value;

	public SendId? SendId { get; set; }

	public FullUri? Target { get; set; }

	public FullUri? Type { get; set; }

#endregion
}