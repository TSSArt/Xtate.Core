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

namespace Xtate.Core.Test;

[TestClass]
public class EventQueueTest
{
	[TestMethod]
	public void EmptyQueueTest()
	{
		var eventQueue = new EventQueue();
		var result = eventQueue.TryReadEvent(out _);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NonEmptyQueueTest()
	{
		var eventQueue = new EventQueue();
		var eventObject = new EventObject();
		await eventQueue.WriteAsync(eventObject);
		var result = eventQueue.TryReadEvent(out var evt);
		var result2 = eventQueue.TryReadEvent(out var evt2);

		Assert.IsTrue(result);
		Assert.IsFalse(result2);
		Assert.AreSame(eventObject, evt);
		Assert.IsNull(evt2);
	}

	[TestMethod]
	public async Task CompleteQueueTest()
	{
		var eventQueue = new EventQueue();
		eventQueue.Complete();
		var result = await eventQueue.WaitToEvent();

		Assert.IsFalse(result);
	}
}