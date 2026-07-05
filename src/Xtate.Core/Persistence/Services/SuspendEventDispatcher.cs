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

namespace Xtate.Persistence.Services;

[InstantiatedByIoC]
public class SuspendEventDispatcher : ISuspendEventDispatcher
{
#region Interface ISuspendEventDispatcher

	event Action? ISuspendEventDispatcher.OnSuspend
	{
		add => OnSuspend += value;
		remove => OnSuspend -= value;
	}

	public bool IsSuspendRequested { get; private set; }

#endregion

	private event Action? OnSuspend;

	public void Suspend(bool setSuspendRequestedFlag)
	{
		if (setSuspendRequestedFlag)
		{
			IsSuspendRequested = true;
		}

		OnSuspend?.Invoke();
	}
}