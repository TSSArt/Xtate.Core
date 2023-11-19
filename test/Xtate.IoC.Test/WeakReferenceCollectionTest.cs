#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test;

[TestClass]
public class WeakReferenceCollectionTest
{
	[TestMethod]
	public void BasicTest()
	{
		var wrc = new WeakReferenceCollection();
		var objects = Enumerable.Range(start: 0, count: 16).Select(_ => new object()).ToList();

		PutList(wrc, objects);

		while (wrc.TryTake(out var obj))
		{
			objects.Remove(obj);
		}

		Assert.AreEqual(expected: 0, objects.Count);
	}

	private static void PutList(WeakReferenceCollection weakReferenceCollection, IEnumerable<object> objects)
	{
		foreach (var o in objects)
		{
			weakReferenceCollection.Put(o);
		}
	}

	[DataTestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(8)]
	[DataRow(16)]
	public void CollectAllTest(int n)
	{
		var wrc = new WeakReferenceCollection();

		PutList(wrc, Enumerable.Range(start: 0, n).Select(_ => new object()));

		GC.Collect();

		var result = wrc.TryTake(out _);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CollectSomeTest()
	{
		var wrc = new WeakReferenceCollection();
		var list = Enumerable.Range(start: 0, count: 8).Select(_ => new object()).ToList();
		PutList(wrc, list);

		list[0] = null!;
		list[1] = null!;
		list[4] = null!;
		list[5] = null!;
		list[7] = null!;

		GC.Collect();

		wrc.Put(new object());

		var count = 0;
		while (wrc.TryTake(out _))
		{
			count ++;
		}

		Assert.AreEqual(expected: 4, count);
	}

	[TestMethod]
	public void IfPutNullShouldRaiseExceptionTest()
	{
		var wrc = new WeakReferenceCollection();

		Assert.ThrowsException<ArgumentNullException>([ExcludeFromCodeCoverage]() => wrc.Put(default!));
	}

	[TestMethod]
	public void MultiThreadTest()
	{
		var wrc = new WeakReferenceCollection();

		var thread1 = new Thread(o => Put((WeakReferenceCollection) o!));
		var thread2 = new Thread(o => Put((WeakReferenceCollection) o!));
		var thread3 = new Thread(o => Take((WeakReferenceCollection) o!));
		var thread4 = new Thread(o => Take((WeakReferenceCollection) o!));

		thread1.Start(wrc);
		thread2.Start(wrc);
		thread3.Start(wrc);
		thread4.Start(wrc);

		thread1.Join();
		thread2.Join();
		thread3.Join();
		thread4.Join();

		while (wrc.TryTake(out _)) { }
	}

	private static void Put(WeakReferenceCollection wrc)
	{
		for (var i = 0; i < 10000; i ++)
		{
			wrc.Put(new object());
		}
	}

	private static void Take(WeakReferenceCollection wrc)
	{
		while (wrc.TryTake(out _)) { }
	}
}