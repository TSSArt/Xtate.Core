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

using System.Threading;
using Xtate.IoProcessors.NamedPipe.Internal;

namespace Xtate.Test.UnitTests.IoProcessors;

[TestClass]
public class DelayedTriesCoverageTest
{
	[TestMethod]
	public async Task DelayCompletesWhenElapsedTimeAlreadyExceedsMinimumDelay()
	{
		var delayedTries = new DelayedTries
						   {
							   MinDelay = TimeSpan.FromMilliseconds(1),
							   MaxDelay = TimeSpan.FromMilliseconds(2)
						   };

		await Task.Delay(20);
		await delayedTries.Delay(CancellationToken.None);
	}

	[TestMethod]
	public async Task DelayUsesCancellationTokenWhenWaiting()
	{
		var delayedTries = new DelayedTries
						   {
							   MinDelay = TimeSpan.FromSeconds(5),
							   MaxDelay = TimeSpan.FromSeconds(5)
						   };
		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await delayedTries.Delay(cancellationTokenSource.Token));
	}

	[TestMethod]
	public void DelayOptionsRejectNonPositiveValues()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new DelayedTries
																	{
																		MinDelay = TimeSpan.Zero,
																		MaxDelay = TimeSpan.FromMilliseconds(1)
																	});

		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new DelayedTries
																	{
																		MinDelay = TimeSpan.FromMilliseconds(1),
																		MaxDelay = TimeSpan.Zero
																	});
	}
}