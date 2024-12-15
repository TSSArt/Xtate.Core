namespace Xtate.Tests
{
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
            var result = InvokeId.New(stateId.Object, null);

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
            var invokeId2 = InvokeId.New(Identifier.FromString("state1"), "Id1");

            // Act
            var result = invokeId1.Equals(invokeId2);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(invokeId1.GetHashCode(), invokeId2.GetHashCode());
        }

        [TestMethod]
        public void New_ShouldReturnDifferentUniqueIds_ForSameInvokeId()
        {
            var invokeId1 = InvokeId.New(Identifier.FromString("state1"), "Invoke");
            var invokeId2 = InvokeId.New(Identifier.FromString("state1"), "Invoke");

            // Act
            var result = invokeId1.Equals(invokeId2);
            var resultUniq = invokeId1.UniqueId.Equals(invokeId2.UniqueId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(resultUniq);
            Assert.AreEqual(invokeId1.GetHashCode(), invokeId2.GetHashCode());
        }
    }
}
