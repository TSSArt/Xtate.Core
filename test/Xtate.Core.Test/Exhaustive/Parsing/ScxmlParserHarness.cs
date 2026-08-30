using System.IO;
using System.Xml;
using Xtate.Scxml.Services;
using Xtate.StateMachine;
using Xtate.StateMachine.Builder;
using Xtate.StateMachine.Builder.Services;
using Xtate.StateMachine.Validator;

namespace Xtate.Core.Test.Exhaustive.Parsing;

/// <summary>
/// Greenfield parser seam: it wires only production parser/builders, keeps diagnostics observable,
/// and deliberately does not use an existing test helper or fixture.
/// </summary>
internal static class ScxmlParserHarness
{
	public static async ValueTask<ParseResult> ParseAsync(string xml, string? baseUri = null)
	{
		var errors = new CollectingErrors();
		using var text = new StringReader(xml);
		using var reader = XmlReader.Create(text, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit }, baseUri);
		return await ParseReaderAsync(reader, errors).ConfigureAwait(false);
	}

	public static async ValueTask<ParseResult> ParseStreamAsync(Stream stream, string? baseUri = null)
	{
		var errors = new CollectingErrors();
		using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit, CloseInput = false }, baseUri);
		return await ParseReaderAsync(reader, errors).ConfigureAwait(false);
	}

	private static async ValueTask<ParseResult> ParseReaderAsync(XmlReader reader, CollectingErrors errors)
	{
		var director = CreateDirector(reader, errors);

		try
		{
			var model = await director.ConstructStateMachine().ConfigureAwait(false);
			return new ParseResult(errors.Messages.Count == 0 ? model : null, errors.Messages, null);
		}
		catch (Exception exception)
		{
			return new ParseResult(null, errors.Messages, exception);
		}
	}

	private static ScxmlDirector CreateDirector(XmlReader reader, CollectingErrors errors) => new(reader)
	{
		StateMachineBuilderFactory = _ => new StateMachineBuilder(),
		StateBuilderFactory = _ => new StateBuilder(),
		ParallelBuilderFactory = _ => new ParallelBuilder(),
		HistoryBuilderFactory = _ => new HistoryBuilder(),
		InitialBuilderFactory = _ => new InitialBuilder(),
		FinalBuilderFactory = _ => new FinalBuilder(),
		TransitionBuilderFactory = _ => new TransitionBuilder(),
		LogBuilderFactory = _ => new LogBuilder(),
		SendBuilderFactory = _ => new SendBuilder(),
		ParamBuilderFactory = _ => new ParamBuilder(),
		ContentBuilderFactory = _ => new ContentBuilder(),
		OnEntryBuilderFactory = _ => new OnEntryBuilder(),
		OnExitBuilderFactory = _ => new OnExitBuilder(),
		InvokeBuilderFactory = _ => new InvokeBuilder(),
		FinalizeBuilderFactory = _ => new FinalizeBuilder(),
		ScriptBuilderFactory = _ => new ScriptBuilder(),
		CustomActionBuilderFactory = _ => new CustomActionBuilder(),
		DataModelBuilderFactory = _ => new DataModelBuilder(),
		DataBuilderFactory = _ => new DataBuilder(),
		DoneDataBuilderFactory = _ => new DoneDataBuilder(),
		AssignBuilderFactory = _ => new AssignBuilder(),
		RaiseBuilderFactory = _ => new RaiseBuilder(),
		CancelBuilderFactory = _ => new CancelBuilder(),
		ForEachBuilderFactory = _ => new ForEachBuilder(),
		IfBuilderFactory = _ => new IfBuilder(),
		ElseBuilderFactory = _ => new ElseBuilder(),
		ElseIfBuilderFactory = _ => new ElseIfBuilder(),
		ErrorProcessorService = errors,
		LineInfoRequired = null
	};

	internal sealed record ParseResult(IStateMachine? Model, IReadOnlyList<string> Diagnostics, Exception? Exception)
	{
		public bool Accepted => Model is not null && Diagnostics.Count == 0 && Exception is null;
	}

	private sealed class CollectingErrors : IErrorProcessorService<ScxmlDirector>
	{
		private readonly List<string> _messages = [];
		public IReadOnlyList<string> Messages => _messages;
		public void AddError(object? entity, string message, Exception? exception = null) => _messages.Add(exception is null ? message : $"{message}: {exception.GetType().Name}");
	}
}
