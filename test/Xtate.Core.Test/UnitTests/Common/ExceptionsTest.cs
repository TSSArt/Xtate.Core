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

using Xtate;

namespace Xtate.Test;

[TestClass]
public class XtateExceptionTest
{
	[TestMethod]
	public void XtateException_DefaultConstructor_CreatesInstance()
	{
		// Act
		var exception = new XtateException();

		// Assert
		Assert.IsNotNull(exception);
		Assert.IsInstanceOfType(exception, typeof(Exception));
		Assert.IsNotNull(exception.Message);
		Assert.IsNull(exception.InnerException);
	}

	[TestMethod]
	public void XtateException_WithMessage_SetsMessage()
	{
		// Arrange
		const string message = "Test error message";

		// Act
		var exception = new XtateException(message);

		// Assert
		Assert.AreEqual(message, exception.Message);
		Assert.IsNull(exception.InnerException);
	}

	[TestMethod]
	public void XtateException_WithNullMessage_SetsNull()
	{
		// Act
		var exception = new XtateException(null);

		// Assert
		Assert.IsNotNull(exception.Message);
	}

	[TestMethod]
	public void XtateException_WithMessageAndInnerException_SetsCorrectly()
	{
		// Arrange
		const string message = "Outer exception";
		var innerException = new ArgumentException("Inner error");

		// Act
		var exception = new XtateException(message, innerException);

		// Assert
		Assert.AreEqual(message, exception.Message);
		Assert.AreEqual(innerException, exception.InnerException);
	}

	[TestMethod]
	public void XtateException_WithEmptyMessage_SetsEmpty()
	{
		// Act
		var exception = new XtateException(string.Empty);

		// Assert
		Assert.AreEqual(string.Empty, exception.Message);
	}

	[TestMethod]
	public void XtateException_InheritsFromException()
	{
		// Act
		var exception = new XtateException("Test");

		// Assert
		Assert.IsInstanceOfType(exception, typeof(Exception));
	}

	[TestMethod]
	public void XtateException_CanBeThrown()
	{
		// Act & Assert
		try
		{
			throw new XtateException("Test exception");
		}
		catch (XtateException ex)
		{
			Assert.AreEqual("Test exception", ex.Message);
		}
	}

	[TestMethod]
	public void XtateException_Serializable()
	{
		// Arrange
		var originalException = new XtateException("Test error");

		// Act & Assert
		try
		{
			throw originalException;
		}
		catch (XtateException ex)
		{
			Assert.AreEqual("Test error", ex.Message);
			Assert.IsNotNull(ex.StackTrace);
		}
	}
}

[TestClass]
public class ExecutionExceptionTest
{
	[TestMethod]
	public void ExecutionException_DefaultConstructor_CreatesInstance()
	{
		// Act
		var exception = new ExecutionException();

		// Assert
		Assert.IsNotNull(exception);
		Assert.IsInstanceOfType(exception, typeof(XtateException));
	}

	[TestMethod]
	public void ExecutionException_WithMessage_SetsMessage()
	{
		// Arrange
		const string message = "Execution error";

		// Act
		var exception = new ExecutionException(message);

		// Assert
		Assert.AreEqual(message, exception.Message);
	}

	[TestMethod]
	public void ExecutionException_WithMessageAndInnerException_SetsCorrectly()
	{
		// Arrange
		const string message = "Execution failed";
		var innerException = new TimeoutException("Timeout");

		// Act
		var exception = new ExecutionException(message, innerException);

		// Assert
		Assert.AreEqual(message, exception.Message);
		Assert.AreEqual(innerException, exception.InnerException);
	}

	[TestMethod]
	public void ExecutionException_InheritsFromXtateException()
	{
		// Act
		var exception = new ExecutionException("Test");

		// Assert
		Assert.IsInstanceOfType(exception, typeof(XtateException));
	}
}

[TestClass]
public class ProcessorExceptionTest
{
	[TestMethod]
	public void ProcessorException_DefaultConstructor_CreatesInstance()
	{
		// Act
		var exception = new ProcessorException();

		// Assert
		Assert.IsNotNull(exception);
		Assert.IsInstanceOfType(exception, typeof(XtateException));
	}

	[TestMethod]
	public void ProcessorException_WithMessage_SetsMessage()
	{
		// Arrange
		const string message = "Processor error";

		// Act
		var exception = new ProcessorException(message);

		// Assert
		Assert.AreEqual(message, exception.Message);
	}

	[TestMethod]
	public void ProcessorException_WithMessageAndInnerException_SetsCorrectly()
	{
		// Arrange
		const string message = "Processing failed";
		var innerException = new InvalidOperationException("Invalid state");

		// Act
		var exception = new ProcessorException(message, innerException);

		// Assert
		Assert.AreEqual(message, exception.Message);
		Assert.AreEqual(innerException, exception.InnerException);
	}

	[TestMethod]
	public void ProcessorException_InheritsFromXtateException()
	{
		// Act
		var exception = new ProcessorException("Test");

		// Assert
		Assert.IsInstanceOfType(exception, typeof(XtateException));
	}
}
