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

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Xtate.DataTypes;
using Xtate.ExternalServices.SmtpClient.Services;
using Xtate.IoC.Tools;
using Xtate.StateMachineHost;
using Xtate.TaskMonitor;

namespace Xtate.Test.UnitTests.ExternalServices;

[TestClass]
public class SmtpClientServiceCoverageTest
{
	[TestMethod]
	public async Task SendsHtmlMessageWithCredentialsInternationalFormatAndConfiguredOptions()
	{
		await using var server = new SmtpLoopbackServer();
		var parameters = new DataModelList(caseInsensitive: true)
						 {
							 ["server"] = "127.0.0.1",
							 ["port"] = server.Port,
							 ["userName"] = "coverage-user",
							 ["password"] = "coverage-password",
							 ["deliveryFormat"] = "international",
							 ["enableSsl"] = false,
							 ["timeout"] = "5000",
							 ["from"] = "sender@example.test",
							 ["to"] = "recipient@example.test",
							 ["body"] = "unused plain body",
							 ["htmlBody"] = "<b>html body</b>",
							 ["subject"] = "Coverage subject"
						 };
		var service = CreateService(parameters);

		var result = await ((IExternalService) service).GetResult();
		var session = await server.Session;

		Assert.IsTrue(result.IsUndefined());
		Assert.IsTrue(session.Commands.Any(static command => command.StartsWith(value: "EHLO ", StringComparison.OrdinalIgnoreCase)));
		Assert.IsTrue(session.Commands.Any(static command => command.StartsWith(value: "AUTH login", StringComparison.OrdinalIgnoreCase)));
		Assert.AreEqual(expected: "coverage-user", session.UserName);
		Assert.AreEqual(expected: "coverage-password", session.Password);
		Assert.IsTrue(session.Commands.Any(static command => command.StartsWith(value: "MAIL FROM:<sender@example.test>", StringComparison.OrdinalIgnoreCase)));
		Assert.IsTrue(session.Commands.Any(static command => command.StartsWith(value: "RCPT TO:<recipient@example.test>", StringComparison.OrdinalIgnoreCase)));
		StringAssert.Contains(session.Message, substring: "Subject: Coverage subject");
		StringAssert.Contains(session.Message, substring: "text/html");
		StringAssert.Contains(session.Message, substring: "PGI+aHRtbCBib2R5PC9iPg==");
	}

	[TestMethod]
	public async Task SendsPlainTextMessageWithDefaultOptionalSettings()
	{
		await using var server = new SmtpLoopbackServer();
		var parameters = new DataModelList
						 {
							 ["server"] = "127.0.0.1",
							 ["port"] = server.Port,
							 ["from"] = "plain-sender@example.test",
							 ["to"] = "plain-recipient@example.test",
							 ["body"] = "plain body"
						 };
		var service = CreateService(parameters);

		var result = await ((IExternalService) service).GetResult();
		var session = await server.Session;

		Assert.IsTrue(result.IsUndefined());
		Assert.IsFalse(session.Commands.Any(static command => command.StartsWith(value: "AUTH ", StringComparison.OrdinalIgnoreCase)));
		Assert.IsFalse(session.Message.Contains(value: "Subject:", StringComparison.Ordinal));
		StringAssert.Contains(session.Message, substring: "text/plain");
		StringAssert.Contains(session.Message, substring: "cGxhaW4gYm9keQ==");
	}

	[TestMethod]
	public async Task InvalidDeliveryFormatIsReportedBeforeConnecting()
	{
		var parameters = new DataModelList
						 {
							 ["server"] = "127.0.0.1",
							 ["port"] = 1,
							 ["deliveryFormat"] = "not-a-delivery-format",
							 ["from"] = "sender@example.test",
							 ["to"] = "recipient@example.test"
						 };
		var service = CreateService(parameters);

		await Assert.ThrowsExactlyAsync<ArgumentException>([ExcludeFromCodeCoverage] async () => await ((IExternalService) service).GetResult());
	}

	[TestMethod]
	public async Task InvalidTimeoutIsReportedBeforeConnecting()
	{
		var parameters = new DataModelList
						 {
							 ["server"] = "127.0.0.1",
							 ["port"] = 1,
							 ["timeout"] = "not-an-integer",
							 ["from"] = "sender@example.test",
							 ["to"] = "recipient@example.test"
						 };
		var service = CreateService(parameters);

		await Assert.ThrowsExactlyAsync<FormatException>([ExcludeFromCodeCoverage] async () => await ((IExternalService) service).GetResult());
	}

	private static SmtpClientService CreateService(DataModelValue parameters)
	{
		var monitor = new PassThroughTaskMonitor();

		return new SmtpClientService
			   {
				   ExternalServiceSourceBase = new ExternalServiceSource(),
				   ExternalServiceParametersBase = new ExternalServiceParameters(parameters),
				   TaskMonitorBase = monitor,
				   DisposeTokenBase = new DisposeToken(CancellationToken.None)
			   };
	}

	private sealed class ExternalServiceSource : IExternalServiceSource
	{
	#region Interface IExternalServiceSource

		public Uri? Source => null;

		public string? RawContent => null;

		public DataModelValue Content => DataModelValue.Undefined;

	#endregion
	}

	private sealed class ExternalServiceParameters(DataModelValue parameters) : IExternalServiceParameters
	{
	#region Interface IExternalServiceParameters

		public DataModelValue Parameters { get; } = parameters;

	#endregion
	}

	private sealed class PassThroughTaskMonitor : ITaskMonitor
	{
	#region Interface ITaskMonitor

		public Task WaitAsync(Task task, CancellationToken token) => task;

		public Task<TResult> WaitAsync<TResult>(Task<TResult> task, CancellationToken token) => task;

		public ValueTask WaitAsync(ValueTask valueTask, CancellationToken token) => valueTask;

		public ValueTask<TResult> WaitAsync<TResult>(ValueTask<TResult> valueTask, CancellationToken token) => valueTask;

		public void Forget(Task task) { }

		public void Forget(ValueTask valueTask) { }

		public void Forget<TResult>(ValueTask<TResult> valueTask) { }

	#endregion
	}

	private sealed class SmtpLoopbackServer : IAsyncDisposable
	{
		private readonly TcpListener _listener = new(IPAddress.Loopback, port: 0);

		public SmtpLoopbackServer()
		{
			_listener.Start();
			Port = ((IPEndPoint) _listener.LocalEndpoint).Port;
			Session = Serve();
		}

		public int Port { get; }

		public Task<SmtpSession> Session { get; }

	#region Interface IAsyncDisposable

		public async ValueTask DisposeAsync()
		{
			_listener.Stop();

			try
			{
				await Session.ConfigureAwait(false);
			}
			catch (SocketException)
			{
				// Stopping a listener that was never reached is expected during assertion failures.
			}
		}

	#endregion

		private async Task<SmtpSession> Serve()
		{
			using var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
			using var stream = client.GetStream();
			using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
			using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 4096, leaveOpen: true)
							   {
								   NewLine = "\r\n",
								   AutoFlush = true
							   };
			var commands = new List<string>();
			var message = new StringBuilder();
			string? userName = null;
			string? password = null;
			var messageAccepted = false;
			await writer.WriteLineAsync("220 localhost ESMTP coverage").ConfigureAwait(false);

			while (await ReadCommandAsync().ConfigureAwait(false) is { } command)
			{
				commands.Add(command);

				if (command.StartsWith(value: "EHLO ", StringComparison.OrdinalIgnoreCase))
				{
					await writer.WriteLineAsync("250-localhost").ConfigureAwait(false);
					await writer.WriteLineAsync("250-AUTH LOGIN").ConfigureAwait(false);
					await writer.WriteLineAsync("250-8BITMIME").ConfigureAwait(false);
					await writer.WriteLineAsync("250 SMTPUTF8").ConfigureAwait(false);
				}
				else if (command.StartsWith(value: "AUTH login", StringComparison.OrdinalIgnoreCase))
				{
					var parts = command.Split([' '], StringSplitOptions.RemoveEmptyEntries);

					if (parts.Length > 2)
					{
						userName = DecodeBase64(parts[2]);
					}
					else
					{
						await writer.WriteLineAsync("334 VXNlcm5hbWU6").ConfigureAwait(false);
						userName = DecodeBase64((await reader.ReadLineAsync().ConfigureAwait(false))!);
					}

					await writer.WriteLineAsync("334 UGFzc3dvcmQ6").ConfigureAwait(false);
					password = DecodeBase64((await reader.ReadLineAsync().ConfigureAwait(false))!);
					await writer.WriteLineAsync("235 2.7.0 Authentication successful").ConfigureAwait(false);
				}
				else if (command.StartsWith(value: "MAIL FROM:", StringComparison.OrdinalIgnoreCase) ||
						 command.StartsWith(value: "RCPT TO:", StringComparison.OrdinalIgnoreCase))
				{
					await writer.WriteLineAsync("250 2.1.0 OK").ConfigureAwait(false);
				}
				else if (command.Equals(value: "DATA", StringComparison.OrdinalIgnoreCase))
				{
					await writer.WriteLineAsync("354 End data with <CR><LF>.<CR><LF>").ConfigureAwait(false);

					while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line && line != ".")
					{
						message.AppendLine(line);
					}

					await writer.WriteLineAsync("250 2.0.0 queued").ConfigureAwait(false);
					messageAccepted = true;
				}
				else if (command.Equals(value: "QUIT", StringComparison.OrdinalIgnoreCase))
				{
					await writer.WriteLineAsync("221 2.0.0 closing connection").ConfigureAwait(false);

					break;
				}
				else
				{
					await writer.WriteLineAsync("250 OK").ConfigureAwait(false);
				}
			}

			return new SmtpSession(commands, message.ToString(), userName, password);

			async Task<string?> ReadCommandAsync()
			{
				try
				{
					return await reader.ReadLineAsync().ConfigureAwait(false);
				}
				catch (IOException exception) when (messageAccepted &&
													exception.InnerException is SocketException
													{
														SocketErrorCode: SocketError.ConnectionAborted or SocketError.ConnectionReset
													})
				{
					// .NET Framework can abort the connection instead of sending QUIT after the message is accepted.

					return null;
				}
			}
		}

		private static string DecodeBase64(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
	}

	private sealed record SmtpSession(
		IReadOnlyList<string> Commands,
		string Message,
		string? UserName,
		string? Password);
}
