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

using System.Net;
using Xtate.IoProcessors.Http;

namespace Xtate.Test;

[TestClass]
public class HttpRequestProcessExceptionTest
{
	[TestMethod]
	public void HttpRequestProcessException_DefaultConstructor_ShouldCreateInstance()
	{
		// Act
		var exception = new HttpRequestProcessException();

		// Assert
		Assert.IsNotNull(exception);
		Assert.IsInstanceOfType(exception, typeof(XtateException));
	}

	[TestMethod]
	public void HttpRequestProcessException_WithMessage_ShouldStoreMessage()
	{
		// Arrange
		var message = "HTTP request processing failed";

		// Act
		var exception = new HttpRequestProcessException(message);

		// Assert
		Assert.AreEqual(message, exception.Message);
	}

	[TestMethod]
	public void HttpRequestProcessException_WithMessageAndInner_ShouldStoreMessageAndInner()
	{
		// Arrange
		var message = "HTTP request processing failed";
		var innerException = new InvalidOperationException("Inner error");

		// Act
		var exception = new HttpRequestProcessException(message, innerException);

		// Assert
		Assert.AreEqual(message, exception.Message);
		Assert.AreEqual(innerException, exception.InnerException);
	}

	[TestMethod]
	public void HttpRequestProcessException_StatusCode_ShouldGetAndSet()
	{
		// Arrange
		var exception = new HttpRequestProcessException();
		var statusCode = HttpStatusCode.NotFound;

		// Act
		exception.StatusCode = statusCode;

		// Assert
		Assert.AreEqual(statusCode, exception.StatusCode);
	}

	[TestMethod]
	public void HttpRequestProcessException_StatusCode_ShouldDefaultToZero()
	{
		// Arrange
		var exception = new HttpRequestProcessException();

		// Act & Assert
		Assert.AreEqual(default(HttpStatusCode), exception.StatusCode);
	}

	[TestMethod]
	public void HttpRequestProcessException_StatusCode_ShouldHandleBadRequest()
	{
		// Arrange
		var exception = new HttpRequestProcessException("Bad request");

		// Act
		exception.StatusCode = HttpStatusCode.BadRequest;

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, exception.StatusCode);
	}

	[TestMethod]
	public void HttpRequestProcessException_StatusCode_ShouldHandleUnauthorized()
	{
		// Arrange
		var exception = new HttpRequestProcessException("Unauthorized");

		// Act
		exception.StatusCode = HttpStatusCode.Unauthorized;

		// Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception.StatusCode);
	}

	[TestMethod]
	public void HttpRequestProcessException_StatusCode_ShouldHandleServerError()
	{
		// Arrange
		var exception = new HttpRequestProcessException("Server error");

		// Act
		exception.StatusCode = HttpStatusCode.InternalServerError;

		// Assert
		Assert.AreEqual(HttpStatusCode.InternalServerError, exception.StatusCode);
	}

	[TestMethod]
	public void HttpRequestProcessException_MultipleStatusCodes_ShouldSupport()
	{
		// Arrange
		var statusCodes = new[]
		{
			HttpStatusCode.OK,
			HttpStatusCode.Created,
			HttpStatusCode.Accepted,
			HttpStatusCode.NoContent,
			HttpStatusCode.MovedPermanently,
			HttpStatusCode.Found,
			HttpStatusCode.NotModified,
			HttpStatusCode.BadRequest,
			HttpStatusCode.Unauthorized,
			HttpStatusCode.Forbidden,
			HttpStatusCode.NotFound,
			HttpStatusCode.Conflict,
			HttpStatusCode.InternalServerError,
			HttpStatusCode.NotImplemented,
			HttpStatusCode.ServiceUnavailable
		};

		// Act & Assert
		foreach (var statusCode in statusCodes)
		{
			var exception = new HttpRequestProcessException();
			exception.StatusCode = statusCode;
			Assert.AreEqual(statusCode, exception.StatusCode);
		}
	}

	[TestMethod]
	public void HttpRequestProcessException_ShouldBeThrowable()
	{
		// Arrange
		var exception = new HttpRequestProcessException("Test exception");

		// Act & Assert
		try
		{
			throw exception;
		}
		catch (HttpRequestProcessException ex)
		{
			Assert.AreEqual("Test exception", ex.Message);
		}
	}

	[TestMethod]
	public void HttpRequestProcessException_WithInnerException_ShouldBeCatchableAsXtateException()
	{
		// Arrange
		var innerException = new TimeoutException("Request timeout");
		var exception = new HttpRequestProcessException("HTTP processing failed", innerException);

		// Act & Assert
		try
		{
			throw exception;
		}
		catch (XtateException ex)
		{
			Assert.AreEqual("HTTP processing failed", ex.Message);
			Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
		}
	}
}
