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

using System.Reflection;
using Xtate.Interpreter;
using Xtate.Interpreter.Services;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Interpreter;

[TestClass]
public class InterpreterModelBuilderEntityMapCoverageTest
{
	[TestMethod]
	public void EntityMapReturnsStoredEntityAndRejectsNullAndOutOfRangeSlots()
	{
		var stored = new EntitySource();
		var type = typeof(InterpreterModelBuilder).GetNestedType(name: "EntityMap", BindingFlags.NonPublic)!;
		var map = (IEntityMap) Activator.CreateInstance(type, [new IEntity?[] { null, stored }])!;

		Assert.IsFalse(map.TryGetEntityByDocumentId(id: 0, out var missing));
		Assert.IsNull(missing);
		Assert.IsTrue(map.TryGetEntityByDocumentId(id: 1, out var actual));
		Assert.AreSame(stored, actual);
		Assert.IsFalse(map.TryGetEntityByDocumentId(id: 2, out var outside));
		Assert.IsNull(outside);
	}

	private sealed class EntitySource : IEntity;
}