using System.IO;
using System.Xml;

namespace Xtate.Core.Test.StateMachine.Types;

[TestClass]
public class EventNameTest
{
    [TestMethod]
    public void EventName_OperatorEquals_ShouldReturnTrueForEqualEventNames()
    {
        // Arrange
        var eventName1 = EventName.FromString("error.execution");
        var eventName2 = EventName.FromString("error.execution");

        // Act
        var result = eventName1 == eventName2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void EventName_OperatorNotEquals_ShouldReturnTrueForDifferentEventNames()
    {
        // Arrange
        var eventName1 = EventName.FromString("error.execution");
        var eventName2 = EventName.FromString("error.communication");

        // Act
        var result = eventName1 != eventName2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void EventName_FromString_ShouldReturnDefaultForNullString()
    {
        // Act
        var eventName = EventName.FromString(null!);

        // Assert
        Assert.IsTrue(eventName.IsDefault);
    }

    [TestMethod]
    public void EventName_FromString_ShouldReturnEmptyForEmptyString()
    {
        // Act
        var eventName = EventName.FromString(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, eventName.ToString());
    }

    [TestMethod]
    public void EventName_GetDoneStateName_ShouldReturnCorrectEventName()
    {
        // Arrange
        var identifier = Identifier.FromString("testState");

        // Act
        var eventName = EventName.GetDoneStateName(identifier);

        // Assert
        Assert.AreEqual("done.state.testState", eventName.ToString());
    }

    [TestMethod]
    public void EventName_GetDoneInvokeName_ShouldReturnCorrectEventName()
    {
        // Arrange
        var invokeId = InvokeId.FromString("testInvoke");

        // Act
        var eventName = EventName.GetDoneInvokeName(invokeId);

        // Assert
        Assert.AreEqual("done.invoke.testInvoke", eventName.ToString());
    }

    [TestMethod]
    public void EventName_GetErrorPlatform_ShouldReturnCorrectEventName()
    {
        // Act
        var eventName = EventName.GetErrorPlatform("testSuffix");

        // Assert
        Assert.AreEqual("error.platform.testSuffix", eventName.ToString());
    }

    [TestMethod]
    public void EventName_Equals_ShouldReturnFalseForDifferentEventNames()
    {
        // Arrange
        var eventName1 = EventName.FromString("error.execution");
        var eventName2 = EventName.FromString("error.communication");

        // Act
        var result = eventName1.Equals(eventName2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void EventName_ToString_ShouldReturnCorrectString()
    {
        // Arrange
        var eventName = EventName.FromString("error.execution");

        // Act
        var result = eventName.ToString();

        // Assert
        Assert.AreEqual("error.execution", result);
    }

    [TestMethod]
    public void EventName_GetHashCode_ShouldReturnSameHashCodeForEqualEventNames()
    {
        // Arrange
        var eventName1 = EventName.FromString("error.execution");
        var eventName2 = EventName.FromString("error.execution");

        // Act
        var hashCode1 = eventName1.GetHashCode();
        var hashCode2 = eventName2.GetHashCode();

        // Assert
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [TestMethod]
    public void EventName_IsError_ShouldReturnTrueForErrorEventName()
    {
        // Arrange
        var eventName = EventName.FromString("error.execution");

        // Act
        var result = eventName.IsError();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void EventName_IsError_ShouldReturnFalseForNonErrorEventName()
    {
        // Arrange
        var eventName = EventName.FromString("done.state");

        // Act
        var result = eventName.IsError();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void EventName_WriteTo_ShouldWriteCorrectXml()
    {
        // Arrange
        var eventName = EventName.FromString("error.execution");
        var stringWriter = new StringWriter();
		var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Auto });

        // Act
        eventName.WriteTo(xmlWriter);
        xmlWriter.Flush();
        var result = stringWriter.ToString();

        // Assert
        Assert.AreEqual("error.execution", result);
    }

    [TestMethod]
    public void EventName_Create_ShouldCreateEventNameFromIdentifiers()
    {
        // Arrange
        var identifiers = new IIdentifier[]
        {
            Identifier.FromString("error"),
            Identifier.FromString("execution")
        };

        // Act
        var eventName = EventName.Create(identifiers);

        // Assert
        Assert.AreEqual("error.execution", eventName.ToString());
    }
}
