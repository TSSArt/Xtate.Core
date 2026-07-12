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
using System.Threading.Tasks;
using Xtate.DataTypes;
using Xtate.IoC.Tools;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class TokenAndLazyValueCoverageTest
{
	[TestMethod]
	public void IdentifierRejectsWhitespaceAndSupportsTryCreateEqualityAndGeneratedIds()
	{
		var identifier = Identifier.FromString("state");
		var same = (Identifier) "state";
		var different = Identifier.FromString("other");

		Assert.AreEqual("state", identifier.Value);
		Assert.IsTrue(identifier.Equals(same));
		Assert.IsTrue(identifier.Equals((object) same));
		Assert.IsFalse(identifier.Equals(different));
		Assert.IsFalse(identifier.Equals("state"));
		Assert.AreEqual(identifier.GetHashCode(), same.GetHashCode());
		Assert.ThrowsExactly<ArgumentException>(() => Identifier.FromString("bad id"));

		Assert.IsTrue(Identifier.TryCreate("new-state", out var created));
		Assert.AreEqual("new-state", created.Value);
		Assert.IsFalse(Identifier.TryCreate(null, out _));
		Assert.IsFalse(Identifier.TryCreate(string.Empty, out _));
		Assert.IsFalse(Identifier.TryCreate("bad id", out _));

		var generated = Identifier.New();
		Assert.IsFalse(string.IsNullOrWhiteSpace(generated.Value));
	}

	[TestMethod]
	public void SendIdSupportsGeneratedExplicitNullAndEqualityPaths()
	{
		var sendId = SendId.FromString("send-0000002a");
		var same = SendId.FromString("send-0000002a");
		var different = SendId.FromString("send-0000002b");

		Assert.IsNotNull(sendId);
		Assert.IsNull(SendId.FromString(null));
		Assert.IsTrue(sendId.Equals(sendId));
		Assert.IsTrue(sendId.Equals(same));
		Assert.IsTrue(sendId.Equals((object) same!));
		Assert.IsFalse(sendId.Equals(different));
		Assert.IsFalse(sendId.Equals("send-0000002a"));
		Assert.AreEqual(42, sendId.GetHashCode());
		Assert.AreEqual("send-0000002a", sendId.ToString());
		Assert.IsFalse(string.IsNullOrWhiteSpace(SendId.New().Value));
	}

	[TestMethod]
	public void InvokeIdSupportsGeneratedExplicitAndUniqueIdentityPaths()
	{
		var stateId = Identifier.FromString("state");
		var explicitInvokeId = InvokeId.New(stateId, "invoke");
		var same = InvokeId.FromString("invoke", explicitInvokeId.UniqueId.Value);
		var unique = InvokeId.FromString("unique");
		var generated = InvokeId.New(stateId, invokeId: null);

		Assert.AreEqual(nameof(InvokeId), explicitInvokeId.ServiceType);
		Assert.AreEqual("invoke", explicitInvokeId.Value);
		Assert.AreNotSame(explicitInvokeId, explicitInvokeId.UniqueId);
		Assert.AreSame(explicitInvokeId, explicitInvokeId.UniqueId.InvokeId);
		Assert.IsTrue(explicitInvokeId.Equals(same));
		Assert.IsTrue(explicitInvokeId.Equals((object) same));
		Assert.IsFalse(explicitInvokeId.Equals(unique));
		Assert.IsFalse(explicitInvokeId.Equals("invoke"));
		Assert.AreSame(unique, unique.UniqueId);
		Assert.AreSame(unique, unique.UniqueId.InvokeId);
		Assert.AreSame(generated, generated.UniqueId);
		Assert.IsFalse(string.IsNullOrWhiteSpace(generated.Value));
		Assert.IsTrue(generated.Value.StartsWith("state.", StringComparison.Ordinal));
	}

	[TestMethod]
	public void SessionIdSupportsGeneratedExplicitNullAndEqualityPaths()
	{
		var sessionId = SessionId.FromString("session-0000002a");
		var same = (SessionId) "session-0000002a";
		var different = SessionId.FromString("session-0000002b");

		Assert.AreEqual(nameof(SessionId), sessionId.ServiceType);
		Assert.IsFalse(SessionId.IsNullOrEmpty(sessionId));
		Assert.IsTrue(SessionId.IsNullOrEmpty(null));
		Assert.IsTrue(SessionId.IsNullOrEmpty(string.Empty));
		Assert.IsTrue(sessionId.Equals(sessionId));
		Assert.IsTrue(sessionId.Equals(same));
		Assert.IsTrue(sessionId.Equals((object) same!));
		Assert.IsFalse(sessionId.Equals(different));
		Assert.IsFalse(sessionId.Equals((object) "session-0000002a"));
		Assert.AreEqual(42, sessionId.GetHashCode());
		Assert.AreEqual("session-0000002a", (string) sessionId);
		string? nullValue = null;
		Assert.IsNull((SessionId?) nullValue);
		Assert.IsFalse(string.IsNullOrWhiteSpace(SessionId.New().Value));
	}

	[TestMethod]
	public void DisposeTokenWrapsCancellationTokenEqualityAndCancellationBehavior()
	{
		using var cancellationTokenSource = new CancellationTokenSource();
		var token = new DisposeToken(cancellationTokenSource.Token);
		var same = new DisposeToken(cancellationTokenSource.Token);
		var different = new DisposeToken(CancellationToken.None);

		Assert.AreEqual(cancellationTokenSource.Token, token.Token);
		Assert.IsFalse(token.IsCancellationRequested);
		Assert.IsTrue(token.Equals(same));
		Assert.IsTrue(token.Equals((object) same));
		Assert.IsTrue(token == same);
		Assert.IsFalse(token != same);
		Assert.IsFalse(token.Equals(different));
		Assert.AreEqual(same.GetHashCode(), token.GetHashCode());
		CancellationToken implicitToken = token;
		Assert.AreEqual(cancellationTokenSource.Token, implicitToken);

		cancellationTokenSource.Cancel();

		Assert.IsTrue(token.IsCancellationRequested);
		Assert.ThrowsExactly<OperationCanceledException>(token.ThrowIfCancellationRequested);
	}

	[TestMethod]
	public async Task DisposingTokenCancelsOnDisposeAndKeepsTokenAccessible()
	{
		var disposingToken = new DisposingToken();
		var token = disposingToken.Token;

		Assert.IsFalse(token.IsCancellationRequested);

		disposingToken.Dispose();

		Assert.IsTrue(token.IsCancellationRequested);
		Assert.IsTrue(disposingToken.Token.IsCancellationRequested);

		await new DisposingToken().DisposeAsync();
	}

	[TestMethod]
	public async Task CancellationTokenRegistrationConfiguredAwaitableDisposesRegistration()
	{
		var called = false;
		using var cancellationTokenSource = new CancellationTokenSource();
		var registration = cancellationTokenSource.Token.Register([ExcludeFromCodeCoverage] () => called = true);

		await registration.ConfigureAwait(continueOnCapturedContext: false).DisposeAsync();
		cancellationTokenSource.Cancel();

		Assert.IsFalse(called);
	}

	[TestMethod]
	public void LazyValueFactoriesEvaluateOnceAndPreserveArguments()
	{
		var noArgCalls = 0;
		var noArg = LazyValue.Create(() =>
									 {
										 noArgCalls ++;

										 return new DataModelValue("created");
									 });

		Assert.AreEqual("created", noArg.AsString());
		Assert.AreEqual("created", noArg.AsString());
		Assert.AreEqual(1, noArgCalls);

		var oneArg = LazyValue.Create("arg", arg => new DataModelValue(arg + "-value"));
		Assert.AreEqual("arg-value", oneArg.AsString());

		var twoArgs = LazyValue.Create("left", "right", (left, right) => new DataModelValue(left + "-" + right));
		Assert.AreEqual("left-right", twoArgs.AsString());
	}
}
