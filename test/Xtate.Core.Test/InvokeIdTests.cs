// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.Tests;

[TestClass]
public class InvokeIdTests
{
    [TestMethod]
    public void FromString_ShouldReturnStaticInvokeId()
    {
        // Arrange
        var invokeId = "testInvokeId";

        // Act
        var result = InvokeId.FromString(invokeId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(UniqueInvokeId));
        Assert.AreEqual(invokeId, result.Value);
        Assert.AreEqual(invokeId.GetHashCode(), result.GetHashCode());
    }

    [TestMethod]
    public void New_ShouldReturnExecutionInvokeId_WhenInvokeIdIsNull()
    {
        // Arrange
        var stateId = new Mock<IIdentifier>();
        stateId.Setup(s => s.Value).Returns("stateIdValue");

        // Act
        var result = InvokeId.New(stateId.Object, invokeId: null);

        // Assert
        Assert.IsInstanceOfType(result, typeof(InvokeId));
        Assert.IsTrue(result.Value.StartsWith("stateIdValue."));
    }

    [TestMethod]
    public void New_ShouldReturnExecutionInvokeId_WhenInvokeIdIsNotNull()
    {
        // Arrange
        var stateId = new Mock<IIdentifier>();
        var invokeId = "testInvokeId";

        // Act
        var result = InvokeId.New(stateId.Object, invokeId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(InvokeId));
        Assert.AreEqual(invokeId, result.Value);
        Assert.AreEqual(invokeId.GetHashCode(), result.GetHashCode());
    }

    [TestMethod]
    public void FromString_WithInvokeUniqueId_ShouldReturnExecutionInvokeId()
    {
        // Arrange
        var invokeId = "testInvokeId";
        var invokeUniqueId = "testInvokeUniqueId";

        // Act
        var result = InvokeId.FromString(invokeId, invokeUniqueId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(InvokeId));
        Assert.AreEqual(invokeId, result.Value);
        Assert.AreEqual(invokeUniqueId, result.UniqueId.Value);
    }

    [TestMethod]
    public void Equals_ShouldReturnTrue_ForSameInvokeId()
    {
        // Arrange
        var invokeId1 = InvokeId.FromString("testInvokeId");
        var invokeId2 = InvokeId.FromString("testInvokeId");

        // Act
        var result = invokeId1.Equals(invokeId2);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(invokeId1.GetHashCode(), invokeId2.GetHashCode());
    }

    [TestMethod]
    public void Equals_ShouldReturnFalse_ForDifferentInvokeId()
    {
        // Arrange
        var invokeId1 = InvokeId.FromString("testInvokeId1");
        var invokeId2 = InvokeId.FromString("testInvokeId2");

        // Act
        var result = invokeId1.Equals(invokeId2);

        // Assert
        Assert.IsFalse(result);
        Assert.AreNotEqual(invokeId1.GetHashCode(), invokeId2.GetHashCode());
    }

    [TestMethod]
    public void Equals_ShouldReturnTrue_ForSameInvokeIdWithDifferentCreationMethods()
    {
        var invokeId1 = InvokeId.FromString("Id1");
        var invokeId2 = InvokeId.New(Identifier.FromString("state1"), invokeId: "Id1");

        // Act
        var result = invokeId1.Equals(invokeId2);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(invokeId1.GetHashCode(), invokeId2.GetHashCode());
    }

    [TestMethod]
    public void New_ShouldReturnDifferentUniqueIds_ForSameInvokeId()
    {
        var invokeId1 = InvokeId.New(Identifier.FromString("state1"), invokeId: "Invoke");
        var invokeId2 = InvokeId.New(Identifier.FromString("state1"), invokeId: "Invoke");

        // Act
        var result = invokeId1.Equals(invokeId2);
        var resultUniq = invokeId1.UniqueId.Equals(invokeId2.UniqueId);

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(resultUniq);
        Assert.AreEqual(invokeId1.GetHashCode(), invokeId2.GetHashCode());
    }
}