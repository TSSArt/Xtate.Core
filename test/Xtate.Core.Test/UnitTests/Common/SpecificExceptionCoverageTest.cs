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

using Xtate.DataModel.XPath;
using Xtate.Interpreter;
using Xtate.Persistence;
using Xtate.StateMachine;
using Xtate.StateMachineHost.Exceptions;

namespace Xtate.Test;

[TestClass]
public class SpecificExceptionCoverageTest
{
	[TestMethod]
	public void CommunicationExceptionConstructorsPreserveMessageInnerExceptionAndSendId()
	{
		var owner = new object();
		var defaultException = new CommunicationException { Owner = owner };
		var messageException = new CommunicationException("communication failed") { Owner = owner };
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new CommunicationException(message: "wrapped", innerException) { Owner = owner };
		var sendId = SendId.FromString("send-1");
		var innerAndSendIdException = new CommunicationException(innerException, sendId) { Owner = owner };

		Assert.IsInstanceOfType(defaultException, typeof(OwnedXtateException));
		Assert.IsTrue(defaultException.IsOwnedBy(owner));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.IsNull(defaultException.SendId);
		Assert.AreEqual(expected: "communication failed", messageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
		Assert.AreSame(innerException, innerAndSendIdException.InnerException);
		Assert.AreSame(sendId, innerAndSendIdException.SendId);
	}

	[TestMethod]
	public void PlatformExceptionConstructorsPreserveMessageAndInnerException()
	{
		var owner = new object();
		var defaultException = new PlatformException { Owner = owner };
		var messageException = new PlatformException("platform failed") { Owner = owner };
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new PlatformException(message: "wrapped", innerException) { Owner = owner };
		var innerOnlyException = new PlatformException(innerException) { Owner = owner };

		Assert.IsInstanceOfType(defaultException, typeof(OwnedXtateException));
		Assert.IsTrue(defaultException.IsOwnedBy(owner));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.AreEqual(expected: "platform failed", messageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
		Assert.AreSame(innerException, innerOnlyException.InnerException);
	}

	[TestMethod]
	public void StateMachineDestroyedExceptionConstructorsPreserveMessageInnerExceptionOwnerAndReason()
	{
		var owner = new object();
		var messageException = new StateMachineDestroyedException("destroyed")
							   {
								   Owner = owner,
								   Reason = DestroyReason.QueueClosed
							   };
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new StateMachineDestroyedException(message: "wrapped", innerException)
									   {
										   Owner = owner,
										   Reason = DestroyReason.LiveLock
									   };

		Assert.IsInstanceOfType(messageException, typeof(OwnedXtateException));
		Assert.IsTrue(messageException.IsOwnedBy(owner));
		Assert.AreEqual(expected: "destroyed", messageException.Message);
		Assert.AreEqual(DestroyReason.QueueClosed, messageException.Reason);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
		Assert.AreEqual(DestroyReason.LiveLock, messageAndInnerException.Reason);
	}

	[TestMethod]
	public void OwnedXtateExceptionConstructorsPreserveMessageInnerExceptionAndOwner()
	{
		var owner = new object();
		var defaultException = new OwnedXtateException { Owner = owner };
		var messageException = new OwnedXtateException("owned") { Owner = owner };
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new OwnedXtateException(message: "wrapped", innerException) { Owner = owner };
		var innerOnlyException = new OwnedXtateException(innerException) { Owner = owner };

		Assert.IsInstanceOfType(defaultException, typeof(XtateException));
		Assert.IsTrue(defaultException.IsOwnedBy(owner));
		Assert.IsFalse(defaultException.IsOwnedBy(new object()));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.AreEqual(expected: "owned", messageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
		Assert.AreSame(innerException, innerOnlyException.InnerException);
	}

	[TestMethod]
	public void XPathDataModelExceptionConstructorsPreserveMessageAndInnerException()
	{
		var defaultException = new XPathDataModelException();
		var messageException = new XPathDataModelException("xpath failed");
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new XPathDataModelException(message: "wrapped", innerException);

		Assert.IsInstanceOfType(defaultException, typeof(XtateException));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.AreEqual(expected: "xpath failed", messageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
	}

	[TestMethod]
	public void PersistenceExceptionConstructorsPreserveMessageAndInnerException()
	{
		var defaultException = new PersistenceException();
		var messageException = new PersistenceException("persistence failed");
		var nullMessageException = new PersistenceException(message: null);
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new PersistenceException(message: "wrapped", innerException);

		Assert.IsInstanceOfType(defaultException, typeof(XtateException));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.AreEqual(expected: "persistence failed", messageException.Message);
		Assert.IsNotNull(nullMessageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
	}

	[TestMethod]
	public void StateMachineSecurityExceptionConstructorsPreserveMessageAndInnerException()
	{
		var defaultException = new StateMachineSecurityException();
		var messageException = new StateMachineSecurityException("security failed");
		var nullMessageException = new StateMachineSecurityException(message: null);
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new StateMachineSecurityException(message: "wrapped", innerException);

		Assert.IsInstanceOfType(defaultException, typeof(XtateException));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.AreEqual(expected: "security failed", messageException.Message);
		Assert.IsNotNull(nullMessageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
	}

	[TestMethod]
	public void StateMachineUnhandledErrorExceptionConstructorsPreserveMessageAndInnerException()
	{
		var defaultException = new StateMachineUnhandledErrorException();
		var messageException = new StateMachineUnhandledErrorException("unhandled");
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new StateMachineUnhandledErrorException(message: "wrapped", innerException);

		Assert.IsInstanceOfType(defaultException, typeof(XtateException));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.AreEqual(expected: "unhandled", messageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
	}

	[TestMethod]
	public void StateMachineSuspendedExceptionConstructorsPreserveMessageInnerExceptionAndOwner()
	{
		var owner = new object();
		var defaultException = new StateMachineSuspendedException { Owner = owner };
		var messageException = new StateMachineSuspendedException("suspended") { Owner = owner };
		var innerException = new InvalidOperationException("inner");
		var messageAndInnerException = new StateMachineSuspendedException(message: "wrapped", innerException) { Owner = owner };

		Assert.IsInstanceOfType(defaultException, typeof(OwnedXtateException));
		Assert.IsTrue(defaultException.IsOwnedBy(owner));
		Assert.IsNotNull(defaultException.Message);
		Assert.IsNull(defaultException.InnerException);
		Assert.AreEqual(expected: "suspended", messageException.Message);
		Assert.AreEqual(expected: "wrapped", messageAndInnerException.Message);
		Assert.AreSame(innerException, messageAndInnerException.InnerException);
	}
}