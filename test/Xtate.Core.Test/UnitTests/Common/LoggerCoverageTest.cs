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

using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xtate.Logging;
using Xtate.Logging.Internal;
using Xtate.Logging.Provider;
using Xtate.Logging.Services;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class LoggerCoverageTest
{
	[TestMethod]
	public void IsEnabledChecksGenericThenNonGenericProvidersAndExposesInvariantFormat()
	{
		var generic = new Mock<ILogProvider<TestSource>>();
		var nonGeneric = new Mock<ILogProvider>();
		var logger = CreateLogger(generic.Object, nonGeneric.Object);

		generic.Setup(p => p.IsEnabled(Level.Warning)).Returns(true);
		Assert.IsTrue(logger.IsEnabled(Level.Warning));
		nonGeneric.Verify(p => p.IsEnabled(typeof(TestSource), Level.Warning), Times.Never);

		generic.Setup(p => p.IsEnabled(Level.Info)).Returns(false);
		nonGeneric.Setup(p => p.IsEnabled(typeof(TestSource), Level.Info)).Returns(true);
		Assert.IsTrue(logger.IsEnabled(Level.Info));

		generic.Setup(p => p.IsEnabled(Level.Debug)).Returns(false);
		nonGeneric.Setup(p => p.IsEnabled(typeof(TestSource), Level.Debug)).Returns(false);
		Assert.IsFalse(logger.IsEnabled(Level.Debug));
		Assert.AreSame(CultureInfo.InvariantCulture, logger.FormatProvider);
	}

	[TestMethod]
	public async Task TraceLogProviderMapsLevelsAndFormatsNullShortAndLongParameterLists()
	{
		var listener = new CapturingTraceListener();
		var provider = new TraceLogProvider<TestSource>([listener]);

		Assert.IsTrue(provider.IsEnabled(Level.Error));
		Assert.IsTrue(provider.IsEnabled(Level.Warning));
		Assert.IsTrue(provider.IsEnabled(Level.Info));
		Assert.IsTrue(provider.IsEnabled(Level.Debug));
		Assert.IsTrue(provider.IsEnabled(Level.Trace));
		Assert.IsTrue(provider.IsEnabled(Level.Verbose));

		await provider.Write(Level.Info, eventId: 10, message: null, parameters: null);
		await provider.Write(Level.Warning, eventId: 11, message: "short", [new LoggingParameter(name: "name", value: "value")]);
		var longParameters = Enumerable.Range(start: 0, count: 17).Select(static i => new LoggingParameter($"p{i}", i));
		await provider.Write(Level.Error, eventId: 12, message: "long", longParameters);

		StringAssert.Contains(listener.Output, substring: "short");
		StringAssert.Contains(listener.Output, substring: "value");
		StringAssert.Contains(listener.Output, substring: "long");
		StringAssert.Contains(listener.Output, substring: "p16");
	}

	[TestMethod]
	public async Task LoggerWritesToGenericAndNonGenericProvidersWithEntityParserAndEnrichers()
	{
		var generic = new CapturingGenericProvider(Level.Info, Level.Debug, Level.Warning);
		var nonGeneric = new CapturingNonGenericProvider(Level.Info, Level.Debug, Level.Warning);
		var parser = new Parser(Level.Debug, new LoggingParameter(name: "entity", value: "parsed"));
		var nullParser = new Parser(Level.Debug, parameter: null);
		var disabledParser = new Parser(Level.Trace, new LoggingParameter(name: "disabled", value: true));
		var namedEnricher = new Enricher(Level.Warning, ns: "context", new LoggingParameter(name: "named", value: 7));
		var fallbackEnricher = new Enricher(Level.Warning, ns: null, new LoggingParameter(name: "fallback", value: 8));
		var disabledEnricher = new Enricher(Level.Trace, ns: "disabled", new LoggingParameter(name: "ignored", value: 9));
		var logger = new Logger<TestSource>
					 {
						 LogWriters = [generic],
						 NonGenericLogWriters = [nonGeneric],
						 LogEnrichers = AsAsync<ILogEnricher<TestSource>>(namedEnricher, fallbackEnricher, disabledEnricher),
						 EntityParserHandlers = AsAsync<IEntityParserHandler>(parser, nullParser, disabledParser)
					 };

		await logger.Write(Level.Info, eventId: 21, message: "plain", entity: "payload");
		await logger.Write(Level.Info, eventId: 22, message: "without entity");

		Assert.HasCount(expected: 2, generic.Entries);
		Assert.HasCount(expected: 2, nonGeneric.Entries);
		Assert.AreEqual(expected: "plain", generic.Entries[0].Message);
		Assert.AreEqual(expected: 21, generic.Entries[0].EventId);
		CollectionAssert.AreEqual(
			new[] { "prop::entity", "context::named", $"{nameof(Enricher)}::fallback" },
			generic.Entries[0].Parameters.Select(static parameter => parameter.FullName()).ToArray());
		CollectionAssert.AreEqual(
			new[] { "context::named", $"{nameof(Enricher)}::fallback" },
			generic.Entries[1].Parameters.Select(static parameter => parameter.FullName()).ToArray());
		CollectionAssert.AreEqual(
			generic.Entries[0].Parameters.Select(static parameter => parameter.FullName()).ToArray(),
			nonGeneric.Entries[0].Parameters.Select(static parameter => parameter.FullName()).ToArray());
		Assert.AreEqual(typeof(TestSource), nonGeneric.Entries[0].Source);
		Assert.AreEqual(expected: 2, parser.Calls);
		Assert.AreEqual(expected: 2, nullParser.Calls);
		Assert.AreEqual(expected: 0, disabledParser.Calls);
		Assert.AreEqual(expected: 4, namedEnricher.Calls);
		Assert.AreEqual(expected: 4, fallbackEnricher.Calls);
		Assert.AreEqual(expected: 0, disabledEnricher.Calls);
	}

	[TestMethod]
	public async Task LoggerInterpolatedWritesCaptureFormatsExpressionsAlignmentAndDisabledShortCircuit()
	{
		var generic = new CapturingGenericProvider(Level.Info);
		var logger = new Logger<TestSource>
					 {
						 LogWriters = [generic],
						 NonGenericLogWriters = [],
						 LogEnrichers = EmptyAsync<ILogEnricher<TestSource>>(),
						 EntityParserHandlers = EmptyAsync<IEntityParserHandler>()
					 };
		var number = 42;
		var formattable = new FormattableValue("formattable");
		var plain = new PlainValue("plain");

		await logger.Write(Level.Info, eventId: 31, $"N={number,5:D3}; F={formattable,-15:U}; P={plain}");
		await logger.Write(Level.Info, eventId: 32, "literal only");

		Assert.HasCount(expected: 2, generic.Entries);
		Assert.AreEqual(expected: "N=  042; F=FORMATTABLE    ; P=plain", generic.Entries[0].Message);
		Assert.HasCount(expected: 3, generic.Entries[0].Parameters);
		Assert.AreEqual(nameof(number), generic.Entries[0].Parameters[0].Name);
		Assert.AreEqual(expected: "D3", generic.Entries[0].Parameters[0].Format);
		Assert.AreSame(formattable, generic.Entries[0].Parameters[1].Value);
		Assert.AreEqual(expected: "U", generic.Entries[0].Parameters[1].Format);
		Assert.AreSame(plain, generic.Entries[0].Parameters[2].Value);
		Assert.AreEqual(expected: "literal only", generic.Entries[1].Message);
		Assert.IsEmpty(generic.Entries[1].Parameters);

		var disabledLogger = new ToggleLogger(enabled: false);
		var disabledHandler = new LoggingInterpolatedStringHandler(literalLength: 4, formattedCount: 1, disabledLogger, Level.Trace, out var shouldFormat);
		Assert.IsFalse(shouldFormat);
		Assert.IsNull(disabledHandler.ToString(out var disabledParameters));
		Assert.IsTrue(disabledParameters.IsDefault);
	}

	private static Logger<TestSource> CreateLogger(ILogProvider<TestSource> generic, ILogProvider nonGeneric) =>
		new()
		{
			LogWriters = [generic],
			NonGenericLogWriters = [nonGeneric],
			LogEnrichers = EmptyAsync<ILogEnricher<TestSource>>(),
			EntityParserHandlers = EmptyAsync<IEntityParserHandler>()
		};

	private static async IAsyncEnumerable<T> EmptyAsync<T>()
	{
		await Task.Yield();

		yield break;
	}

	private static async IAsyncEnumerable<T> AsAsync<T>(params T[] items)
	{
		await Task.Yield();

		foreach (var item in items)
		{
			yield return item;
		}
	}

	private sealed class CapturingTraceListener : TraceListener
	{
		private readonly StringBuilder _output = new();

		public string Output => _output.ToString();

		public override void Write(string? message) => _output.Append(message);

		public override void WriteLine(string? message) => _output.AppendLine(message);
	}

	public sealed class TestSource;

	private sealed class CapturingGenericProvider(params Level[] enabledLevels) : ILogProvider<TestSource>
	{
		private readonly HashSet<Level> _enabledLevels = [.. enabledLevels];

		public List<GenericEntry> Entries { get; } = [];

	#region Interface ILogProvider<TestSource>

		public bool IsEnabled(Level level) => _enabledLevels.Contains(level);

		public ValueTask Write(Level level,
							   int eventId,
							   string? message,
							   IEnumerable<LoggingParameter>? parameters = null)
		{
			Entries.Add(new GenericEntry(level, eventId, message, parameters?.ToArray() ?? []));

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class CapturingNonGenericProvider(params Level[] enabledLevels) : ILogProvider
	{
		private readonly HashSet<Level> _enabledLevels = [.. enabledLevels];

		public List<NonGenericEntry> Entries { get; } = [];

	#region Interface ILogProvider

		public bool IsEnabled(Type source, Level level) => _enabledLevels.Contains(level);

		public ValueTask Write(Type source,
							   Level level,
							   int eventId,
							   string? message,
							   IEnumerable<LoggingParameter>? parameters = null)
		{
			Entries.Add(new NonGenericEntry(source, level, eventId, message, parameters?.ToArray() ?? []));

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class Parser(Level level, LoggingParameter? parameter) : IEntityParserHandler
	{
		public int Calls { get; private set; }

	#region Interface IEntityParserHandler

		public IEnumerable<LoggingParameter>? EnumerateProperties<T>(T entity)
		{
			Calls ++;

			return parameter is { } value ? [value] : null;
		}

		public Level Level { get; } = level;

	#endregion
	}

	private sealed class Enricher(Level level, string? ns, LoggingParameter parameter) : ILogEnricher<TestSource>
	{
		public int Calls { get; private set; }

	#region Interface ILogEnricher<TestSource>

		public IEnumerable<LoggingParameter> EnumerateProperties()
		{
			Calls ++;

			return [parameter];
		}

		public string? Namespace { get; } = ns;

		public Level Level { get; } = level;

	#endregion
	}

	private sealed class ToggleLogger(bool enabled) : ILogger
	{
	#region Interface ILogger

		public bool IsEnabled(Level level) => enabled;

		public IFormatProvider? FormatProvider => CultureInfo.InvariantCulture;

	#endregion
	}

	private sealed class FormattableValue(string value) : IFormattable
	{
	#region Interface IFormattable

		public string ToString(string? format, IFormatProvider? formatProvider) => format == "U" ? value.ToUpperInvariant() : value;

	#endregion
	}

	private sealed class PlainValue(string value)
	{
		public override string ToString() => value;
	}

	private sealed record GenericEntry(
		Level Level,
		int EventId,
		string? Message,
		LoggingParameter[] Parameters);

	private sealed record NonGenericEntry(
		Type Source,
		Level Level,
		int EventId,
		string? Message,
		LoggingParameter[] Parameters);
}