﻿// Copyright © 2019-2024 Sergii Artemenko
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

using System.Buffers;
using System.Globalization;
using System.Xml;
using Xtate.Builder;

namespace Xtate.Scxml;

public class ScxmlDirector : XmlDirector<ScxmlDirector>
{
	private const char Space = ' ';

	private static readonly char[] SpaceSplitter = [Space];

	private static readonly Policy<IStateMachineBuilder> StateMachinePolicy = BuildPolicy<IStateMachineBuilder>(StateMachineBuildPolicy);

	private static readonly Policy<IStateBuilder> StatePolicy = BuildPolicy<IStateBuilder>(StateBuildPolicy);

	private static readonly Policy<IParallelBuilder> ParallelPolicy = BuildPolicy<IParallelBuilder>(ParallelBuildPolicy);

	private static readonly Policy<IFinalBuilder> FinalPolicy = BuildPolicy<IFinalBuilder>(FinalBuildPolicy);

	private static readonly Policy<IInitialBuilder> InitialPolicy = BuildPolicy<IInitialBuilder>(InitialBuildPolicy);

	private static readonly Policy<IHistoryBuilder> HistoryPolicy = BuildPolicy<IHistoryBuilder>(HistoryBuildPolicy);

	private static readonly Policy<ITransitionBuilder> TransitionPolicy = BuildPolicy<ITransitionBuilder>(TransitionBuildPolicy);

	private static readonly Policy<ILogBuilder> LogPolicy = BuildPolicy<ILogBuilder>(LogBuildPolicy);

	private static readonly Policy<ISendBuilder> SendPolicy = BuildPolicy<ISendBuilder>(SendBuildPolicy);

	private static readonly Policy<IParamBuilder> ParamPolicy = BuildPolicy<IParamBuilder>(ParamBuildPolicy);

	private static readonly Policy<IContentBuilder> ContentPolicy = BuildPolicy<IContentBuilder>(ContentBuildPolicy);

	private static readonly Policy<IOnEntryBuilder> OnEntryPolicy = BuildPolicy<IOnEntryBuilder>(OnEntryBuildPolicy);

	private static readonly Policy<IOnExitBuilder> OnExitPolicy = BuildPolicy<IOnExitBuilder>(OnExitBuildPolicy);

	private static readonly Policy<IInvokeBuilder> InvokePolicy = BuildPolicy<IInvokeBuilder>(InvokeBuildPolicy);

	private static readonly Policy<IFinalizeBuilder> FinalizePolicy = BuildPolicy<IFinalizeBuilder>(FinalizeBuildPolicy);

	private static readonly Policy<IScriptBuilder> ScriptPolicy = BuildPolicy<IScriptBuilder>(ScriptBuildPolicy);

	private static readonly Policy<IDataModelBuilder> DataModelPolicy = BuildPolicy<IDataModelBuilder>(DataModelBuildPolicy);

	private static readonly Policy<IDataBuilder> DataPolicy = BuildPolicy<IDataBuilder>(DataBuildPolicy);

	private static readonly Policy<IDoneDataBuilder> DoneDataPolicy = BuildPolicy<IDoneDataBuilder>(DoneDataBuildPolicy);

	private static readonly Policy<IForEachBuilder> ForEachPolicy = BuildPolicy<IForEachBuilder>(ForEachBuildPolicy);

	private static readonly Policy<IIfBuilder> IfPolicy = BuildPolicy<IIfBuilder>(IfBuildPolicy);

	private static readonly Policy<IElseBuilder> ElsePolicy = BuildPolicy<IElseBuilder>(ElseBuildPolicy);

	private static readonly Policy<IElseIfBuilder> ElseIfPolicy = BuildPolicy<IElseIfBuilder>(ElseIfBuildPolicy);

	private static readonly Policy<IRaiseBuilder> RaisePolicy = BuildPolicy<IRaiseBuilder>(RaiseBuildPolicy);

	private static readonly Policy<IAssignBuilder> AssignPolicy = BuildPolicy<IAssignBuilder>(AssignBuildPolicy);

	private static readonly Policy<ICancelBuilder> CancelPolicy = BuildPolicy<ICancelBuilder>(CancelBuildPolicy);

	private readonly XmlReader _xmlReader;

	private List<string>? _namespacePrefixes;

	private List<ImmutableArray<PrefixNamespace>>? _nsCache;

	public ScxmlDirector(XmlReader xmlReader) : base(xmlReader)
	{
		_xmlReader = xmlReader;

		var nameTable = xmlReader.NameTable;

		Infra.Requires(nameTable);

		FillNameTable(nameTable);
	}

	public required Func<object?, IStateMachineBuilder> StateMachineBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IStateBuilder> StateBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IParallelBuilder> ParallelBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IHistoryBuilder> HistoryBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IInitialBuilder> InitialBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IFinalBuilder> FinalBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, ITransitionBuilder> TransitionBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, ILogBuilder> LogBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, ISendBuilder> SendBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IParamBuilder> ParamBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IContentBuilder> ContentBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IOnEntryBuilder> OnEntryBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IOnExitBuilder> OnExitBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IInvokeBuilder> InvokeBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IFinalizeBuilder> FinalizeBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IScriptBuilder> ScriptBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, ICustomActionBuilder> CustomActionBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IDataModelBuilder> DataModelBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IDataBuilder> DataBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IDoneDataBuilder> DoneDataBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IAssignBuilder> AssignBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IRaiseBuilder> RaiseBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, ICancelBuilder> CancelBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IForEachBuilder> ForEachBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IIfBuilder> IfBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IElseBuilder> ElseBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<object?, IElseIfBuilder> ElseIfBuilderFactory { private get; [UsedImplicitly] init; }

	public required IErrorProcessorService<ScxmlDirector> ErrorProcessorService { private get; [UsedImplicitly] init; }

	public required ILineInfoRequired? LineInfoRequired { private get; [UsedImplicitly] init; }

	private static void FillNameTable(XmlNameTable nameTable)
	{
		StateMachinePolicy.FillNameTable(nameTable);
		StatePolicy.FillNameTable(nameTable);
		ParallelPolicy.FillNameTable(nameTable);
		FinalPolicy.FillNameTable(nameTable);
		InitialPolicy.FillNameTable(nameTable);
		HistoryPolicy.FillNameTable(nameTable);
		TransitionPolicy.FillNameTable(nameTable);
		LogPolicy.FillNameTable(nameTable);
		SendPolicy.FillNameTable(nameTable);
		ParamPolicy.FillNameTable(nameTable);
		ContentPolicy.FillNameTable(nameTable);
		OnEntryPolicy.FillNameTable(nameTable);
		OnExitPolicy.FillNameTable(nameTable);
		InvokePolicy.FillNameTable(nameTable);
		FinalizePolicy.FillNameTable(nameTable);
		ScriptPolicy.FillNameTable(nameTable);
		DataModelPolicy.FillNameTable(nameTable);
		DataPolicy.FillNameTable(nameTable);
		DoneDataPolicy.FillNameTable(nameTable);
		ForEachPolicy.FillNameTable(nameTable);
		IfPolicy.FillNameTable(nameTable);
		ElsePolicy.FillNameTable(nameTable);
		ElseIfPolicy.FillNameTable(nameTable);
		RaisePolicy.FillNameTable(nameTable);
		AssignPolicy.FillNameTable(nameTable);
		CancelPolicy.FillNameTable(nameTable);
	}

	protected override void NamespaceAttribute(string newPrefix)
	{
		_namespacePrefixes ??= [];

		foreach (var prefix in _namespacePrefixes)
		{
			if (ReferenceEquals(prefix, newPrefix))
			{
				return;
			}
		}

		_namespacePrefixes.Add(newPrefix);
	}

	protected override void OnError(string message, Exception? exception) => ErrorProcessorService.AddError(CreateXmlLineInfo(), message, exception);

	public ValueTask<IStateMachine> ConstructStateMachine() => ReadStateMachine();

	private static IIdentifier AsIdentifier(string value)
	{
		Infra.Requires(value);

		return (Identifier) value;
	}

	private static IOutgoingEvent AsEvent(string value)
	{
		Infra.Requires(value);

		return new EventEntity(value) { Target = Const.InternalTarget };
	}

	private static ImmutableArray<IIdentifier> AsIdentifierList(string value)
	{
		Infra.Requires(value);

		if (value.Length == 0)
		{
			throw new ArgumentException(Resources.Exception_ListOfIdentifiersCannotBeEmpty, nameof(value));
		}

		if (value.IndexOf(Space) < 0)
		{
			return [Identifier.FromString(value)];
		}

		var identifiers = value.Split(SpaceSplitter, StringSplitOptions.None);

		var builder = ImmutableArray.CreateBuilder<IIdentifier>(identifiers.Length);

		foreach (var identifier in identifiers)
		{
			builder.Add((Identifier) identifier);
		}

		return builder.MoveToImmutable();
	}

	private static ImmutableArray<IEventDescriptor> AsEventDescriptorList(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentException(Resources.Exception_ListOfEventsCannotBeEmpty, nameof(value));
		}

		if (value.IndexOf(Space) < 0)
		{
			return [EventDescriptor.FromString(value)];
		}

		var eventDescriptors = value.Split(SpaceSplitter, StringSplitOptions.RemoveEmptyEntries);

		if (eventDescriptors.Length == 0)
		{
			throw new ArgumentException(Resources.Exception_ListOfEventsCannotBeEmpty, nameof(value));
		}

		var builder = ImmutableArray.CreateBuilder<IEventDescriptor>(eventDescriptors.Length);

		foreach (var identifier in eventDescriptors)
		{
			builder.Add(EventDescriptor.FromString(identifier));
		}

		return builder.MoveToImmutable();
	}

	private IConditionExpression AsConditionalExpression(string expression)
	{
		if (string.IsNullOrEmpty(expression))
		{
			throw new ArgumentException(Resources.Exception_ConditionDoesNotSpecify, nameof(expression));
		}

		return new ConditionExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true) };
	}

	private ILocationExpression AsLocationExpression(string expression)
	{
		if (string.IsNullOrEmpty(expression))
		{
			throw new ArgumentException(Resources.Exception_LocationDoesNotSpecify, nameof(expression));
		}

		return new LocationExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true) };
	}

	private ImmutableArray<ILocationExpression> AsLocationExpressionList(string expression)
	{
		if (string.IsNullOrEmpty(expression))
		{
			throw new ArgumentException(Resources.Exception_ListOfLocationsCannotBeEmpty, nameof(expression));
		}

		if (expression.IndexOf(Space) < 0)
		{
			return [new LocationExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true) }];
		}

		var locationExpressions = expression.Split(SpaceSplitter, StringSplitOptions.RemoveEmptyEntries);

		if (locationExpressions.Length == 0)
		{
			throw new ArgumentException(Resources.Exception_ListOfLocationsCannotBeEmpty, nameof(expression));
		}

		var builder = ImmutableArray.CreateBuilder<ILocationExpression>(locationExpressions.Length);

		foreach (var locationExpression in locationExpressions)
		{
			builder.Add(new LocationExpression { Expression = locationExpression, Ancestor = CreateAncestor(namespaces: true) });
		}

		return builder.MoveToImmutable();
	}

	private IValueExpression AsValueExpression(string expression)
	{
		Infra.Requires(expression);

		return new ValueExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true) };
	}

	private IScriptExpression AsScriptExpression(string expression)
	{
		Infra.Requires(expression);

		return new ScriptExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true) };
	}

	private IInlineContent AsInlineContent(string inlineContent)
	{
		Infra.Requires(inlineContent);

		return new InlineContent { Value = inlineContent, Ancestor = CreateAncestor(namespaces: true) };
	}

	private IContentBody AsContentBody(string contentBody)
	{
		Infra.Requires(contentBody);

		return new ContentBody { Value = contentBody, Ancestor = CreateAncestor(namespaces: true) };
	}

	private static IExternalScriptExpression AsExternalScriptExpression(string uri)
	{
		Infra.Requires(uri);

		return new ExternalScriptExpression { Uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
	}

	private static IExternalDataExpression AsExternalDataExpression(string uri)
	{
		Infra.Requires(uri);

		return new ExternalDataExpression { Uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
	}

	private static Uri AsUri(string uri)
	{
		Infra.Requires(uri);

		return new Uri(uri, UriKind.RelativeOrAbsolute);
	}

	private static T AsEnum<T>(string value) where T : struct
	{
		Infra.Requires(value);

		if (!Enum.TryParse(value, ignoreCase: true, out T result) || value.Any(char.IsUpper))
		{
			throw new ArgumentException(Res.Format(Resources.Exception_ValueCannotBeParsed, typeof(T).Name));
		}

		return result;
	}

	private static int AsMilliseconds(string value)
	{
		Infra.RequiresNonEmptyString(value);

		if (value == @"0")
		{
			return 0;
		}

		const string ms = "ms";

		if (value.EndsWith(ms, StringComparison.Ordinal))
		{
			return int.Parse(value[..^ms.Length], NumberFormatInfo.InvariantInfo);
		}

		const string s = "s";

		if (value.EndsWith(s, StringComparison.Ordinal))
		{
			return int.Parse(value[..^s.Length], NumberFormatInfo.InvariantInfo) * 1000;
		}

		throw new ArgumentException(Resources.Exception_DelayParsingError);
	}

	private static void CheckScxmlVersion(string version)
	{
		if (version == @"1.0")
		{
			return;
		}

		throw new ArgumentException(Resources.Exception_UnsupportedScxmlVersion);
	}

	private object? CreateAncestor(bool namespaces = false)
	{
		var result = CreateXmlLineInfo();

		if (namespaces)
		{
			result = CreateXmlNamespacesInfo(result);
		}

		return result;
	}

	private object? CreateXmlLineInfo(object? ancestor = default) =>
		LineInfoRequired?.LineInfoRequired == true && _xmlReader is IXmlLineInfo xmlLineInfo && xmlLineInfo.HasLineInfo()
			? new XmlLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition, ancestor)
			: ancestor;

	private object? CreateXmlNamespacesInfo(object? ancestor)
	{
		if (_namespacePrefixes is null)
		{
			return ancestor;
		}

		PrefixNamespace[]? namespaces = default;

		var count = 0;

		foreach (var prefix in _namespacePrefixes)
		{
			if (_xmlReader.LookupNamespace(prefix) is { Length: > 0 } val)
			{
				namespaces ??= ArrayPool<PrefixNamespace>.Shared.Rent(_namespacePrefixes.Count);
				namespaces[count ++] = new PrefixNamespace(prefix, val);
			}
		}

		if (namespaces is null)
		{
			return ancestor;
		}

		var array = ResolveThroughCache(namespaces, count);

		Array.Clear(namespaces, index: 0, count);
		ArrayPool<PrefixNamespace>.Shared.Return(namespaces);

		return new XmlNamespacesInfo(array, ancestor);
	}

	private ImmutableArray<PrefixNamespace> ResolveThroughCache(PrefixNamespace[] list, int count)
	{
		_nsCache ??= [];

		foreach (var item in _nsCache)
		{
			if (CompareArrays(item, list, count))
			{
				return item;
			}
		}

		var array = ImmutableArray.Create(list, start: 0, count);

		_nsCache.Add(array);

		return array;
	}

	private static bool CompareArrays(ImmutableArray<PrefixNamespace> array, PrefixNamespace[] list, int count)
	{
		if (array.Length != count)
		{
			return false;
		}

		for (var i = 0; i < count; i ++)
		{
			if (!ReferenceEquals(list[i].Prefix, array[i].Prefix) || list[i].Namespace != array[i].Namespace)
			{
				return false;
			}
		}

		return true;
	}

	private async ValueTask<IStateMachine> ReadStateMachine() => (await Populate(StateMachineBuilderFactory(CreateAncestor()), StateMachinePolicy).ConfigureAwait(false)).Build();

	private static void StateMachineBuildPolicy(IPolicyBuilder<IStateMachineBuilder> pb) =>
		pb.ValidateElementName(Const.ScxmlNs, name: "scxml")
		  .RequiredAttribute(name: "version", (dr, _) => CheckScxmlVersion(dr.AttributeValue))
		  .OptionalAttribute(name: "initial", (dr, b) => b.SetInitial(AsIdentifierList(dr.AttributeValue)))
		  .OptionalAttribute(name: "datamodel", (dr, b) => b.SetDataModelType(dr.AttributeValue))
		  .OptionalAttribute(name: "binding", (dr, b) => b.SetBindingType(AsEnum<BindingType>(dr.AttributeValue)))
		  .OptionalAttribute(name: "name", (dr, b) => b.SetName(dr.AttributeValue))
		  .MultipleElements(Const.ScxmlNs, name: "state", async (dr, b) => b.AddState(await dr.ReadState().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "parallel", async (dr, b) => b.AddParallel(await dr.ReadParallel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "final", async (dr, b) => b.AddFinal(await dr.ReadFinal().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "datamodel", async (dr, b) => b.SetDataModel(await dr.ReadDataModel().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "script", async (dr, b) => b.SetScript(await dr.ReadScript().ConfigureAwait(false)))
		  .OptionalAttribute(Const.XtateScxmlNs, name: "synchronous", (dr, b) => b.SetSynchronousEventProcessing(XmlConvert.ToBoolean(dr.AttributeValue)))
		  .OptionalAttribute(Const.XtateScxmlNs, name: "queueSize", (dr, b) => b.SetExternalQueueSize(XmlConvert.ToInt32(dr.AttributeValue)))
		  .OptionalAttribute(Const.XtateScxmlNs, name: "persistence", (dr, b) => b.SetPersistenceLevel((PersistenceLevel) Enum.Parse(typeof(PersistenceLevel), dr.AttributeValue)))
		  .OptionalAttribute(Const.XtateScxmlNs, name: "onError", (dr, b) => b.SetUnhandledErrorBehaviour((UnhandledErrorBehaviour) Enum.Parse(typeof(UnhandledErrorBehaviour), dr.AttributeValue)));

	private async ValueTask<IState> ReadState() => (await Populate(StateBuilderFactory(CreateAncestor()), StatePolicy).ConfigureAwait(false)).Build();

	private static void StateBuildPolicy(IPolicyBuilder<IStateBuilder> pb) =>
		pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
		  .OptionalAttribute(name: "initial", (dr, b) => b.SetInitial(AsIdentifierList(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "state", async (dr, b) => b.AddState(await dr.ReadState().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "parallel", async (dr, b) => b.AddParallel(await dr.ReadParallel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "final", async (dr, b) => b.AddFinal(await dr.ReadFinal().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "history", async (dr, b) => b.AddHistory(await dr.ReadHistory().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "invoke", async (dr, b) => b.AddInvoke(await dr.ReadInvoke().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "transition", async (dr, b) => b.AddTransition(await dr.ReadTransition().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "onentry", async (dr, b) => b.AddOnEntry(await dr.ReadOnEntry().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "onexit", async (dr, b) => b.AddOnExit(await dr.ReadOnExit().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "initial", async (dr, b) => b.SetInitial(await dr.ReadInitial().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "datamodel", async (dr, b) => b.SetDataModel(await dr.ReadDataModel().ConfigureAwait(false)));

	private async ValueTask<IParallel> ReadParallel() => (await Populate(ParallelBuilderFactory(CreateAncestor()), ParallelPolicy).ConfigureAwait(false)).Build();

	private static void ParallelBuildPolicy(IPolicyBuilder<IParallelBuilder> pb) =>
		pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "state", async (dr, b) => b.AddState(await dr.ReadState().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "parallel", async (dr, b) => b.AddParallel(await dr.ReadParallel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "history", async (dr, b) => b.AddHistory(await dr.ReadHistory().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "invoke", async (dr, b) => b.AddInvoke(await dr.ReadInvoke().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "transition", async (dr, b) => b.AddTransition(await dr.ReadTransition().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "onentry", async (dr, b) => b.AddOnEntry(await dr.ReadOnEntry().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "onexit", async (dr, b) => b.AddOnExit(await dr.ReadOnExit().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "datamodel", async (dr, b) => b.SetDataModel(await dr.ReadDataModel().ConfigureAwait(false)));

	private async ValueTask<IFinal> ReadFinal() => (await Populate(FinalBuilderFactory(CreateAncestor()), FinalPolicy).ConfigureAwait(false)).Build();

	private static void FinalBuildPolicy(IPolicyBuilder<IFinalBuilder> pb) =>
		pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "onentry", async (dr, b) => b.AddOnEntry(await dr.ReadOnEntry().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "onexit", async (dr, b) => b.AddOnExit(await dr.ReadOnExit().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "donedata", async (dr, b) => b.SetDoneData(await dr.ReadDoneData().ConfigureAwait(false)));

	private async ValueTask<IInitial> ReadInitial() => (await Populate(InitialBuilderFactory(CreateAncestor()), InitialPolicy).ConfigureAwait(false)).Build();

	private static void InitialBuildPolicy(IPolicyBuilder<IInitialBuilder> pb) =>
		pb.SingleElement(Const.ScxmlNs, name: "transition", async (dr, b) => b.SetTransition(await dr.ReadTransition().ConfigureAwait(false)));

	private async ValueTask<IHistory> ReadHistory() => (await Populate(HistoryBuilderFactory(CreateAncestor()), HistoryPolicy).ConfigureAwait(false)).Build();

	private static void HistoryBuildPolicy(IPolicyBuilder<IHistoryBuilder> pb) =>
		pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
		  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsEnum<HistoryType>(dr.AttributeValue)))
		  .SingleElement(Const.ScxmlNs, name: "transition", async (dr, b) => b.SetTransition(await dr.ReadTransition().ConfigureAwait(false)));

	private async ValueTask<ITransition> ReadTransition() => (await Populate(TransitionBuilderFactory(CreateAncestor()), TransitionPolicy).ConfigureAwait(false)).Build();

	private static void TransitionBuildPolicy(IPolicyBuilder<ITransitionBuilder> pb) =>
		pb.OptionalAttribute(name: "event", (dr, b) => b.SetEvent(AsEventDescriptorList(dr.AttributeValue)))
		  .OptionalAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "target", (dr, b) => b.SetTarget(AsIdentifierList(dr.AttributeValue)))
		  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsEnum<TransitionType>(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
		  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

	private async ValueTask<ILog> ReadLog() => (await Populate(LogBuilderFactory(CreateAncestor()), LogPolicy).ConfigureAwait(false)).Build();

	private static void LogBuildPolicy(IPolicyBuilder<ILogBuilder> pb) =>
		pb.OptionalAttribute(name: "label", (dr, b) => b.SetLabel(dr.AttributeValue))
		  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)));

	private async ValueTask<ISend> ReadSend() => (await Populate(SendBuilderFactory(CreateAncestor()), SendPolicy).ConfigureAwait(false)).Build();

	private static void SendBuildPolicy(IPolicyBuilder<ISendBuilder> pb) =>
		pb.OptionalAttribute(name: "event", (dr, b) => b.SetEvent(dr.AttributeValue))
		  .OptionalAttribute(name: "eventexpr", (dr, b) => b.SetEventExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "target", (dr, b) => b.SetTarget(new FullUri(dr.AttributeValue)))
		  .OptionalAttribute(name: "targetexpr", (dr, b) => b.SetTargetExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "type", (dr, b) => b.SetType(new FullUri(dr.AttributeValue)))
		  .OptionalAttribute(name: "typeexpr", (dr, b) => b.SetTypeExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "id", (dr, b) => b.SetId(dr.AttributeValue))
		  .OptionalAttribute(name: "idlocation", (dr, b) => b.SetIdLocation(dr.AsLocationExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "delay", (dr, b) => b.SetDelay(AsMilliseconds(dr.AttributeValue)))
		  .OptionalAttribute(name: "delayexpr", (dr, b) => b.SetDelayExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "namelist", (dr, b) => b.SetNameList(dr.AsLocationExpressionList(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "param", async (dr, b) => b.AddParameter(await dr.ReadParam().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "content", async (dr, b) => b.SetContent(await dr.ReadContent().ConfigureAwait(false)));

	private async ValueTask<IParam> ReadParam() => (await Populate(ParamBuilderFactory(CreateAncestor()), ParamPolicy).ConfigureAwait(false)).Build();

	private static void ParamBuildPolicy(IPolicyBuilder<IParamBuilder> pb) =>
		pb.RequiredAttribute(name: "name", (dr, b) => b.SetName(dr.AttributeValue))
		  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "location", (dr, b) => b.SetLocation(dr.AsLocationExpression(dr.AttributeValue)));

	private async ValueTask<IContent> ReadContent() => (await Populate(ContentBuilderFactory(CreateAncestor()), ContentPolicy).ConfigureAwait(false)).Build();

	private static void ContentBuildPolicy(IPolicyBuilder<IContentBuilder> pb) =>
		pb.OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .RawContent((dr, b) => b.SetBody(dr.AsContentBody(dr.RawContent)));

	private async ValueTask<IOnEntry> ReadOnEntry() => (await Populate(OnEntryBuilderFactory(CreateAncestor()), OnEntryPolicy).ConfigureAwait(false)).Build();

	private static void OnEntryBuildPolicy(IPolicyBuilder<IOnEntryBuilder> pb) =>
		pb.MultipleElements(Const.ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
		  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

	private async ValueTask<IOnExit> ReadOnExit() => (await Populate(OnExitBuilderFactory(CreateAncestor()), OnExitPolicy).ConfigureAwait(false)).Build();

	private static void OnExitBuildPolicy(IPolicyBuilder<IOnExitBuilder> pb) =>
		pb.MultipleElements(Const.ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
		  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

	private async ValueTask<IInvoke> ReadInvoke() => (await Populate(InvokeBuilderFactory(CreateAncestor()), InvokePolicy).ConfigureAwait(false)).Build();

	private static void InvokeBuildPolicy(IPolicyBuilder<IInvokeBuilder> pb) =>
		pb.OptionalAttribute(name: "type", (dr, b) => b.SetType(new FullUri(dr.AttributeValue)))
		  .OptionalAttribute(name: "typeexpr", (dr, b) => b.SetTypeExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsUri(dr.AttributeValue)))
		  .OptionalAttribute(name: "srcexpr", (dr, b) => b.SetSourceExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "id", (dr, b) => b.SetId(dr.AttributeValue))
		  .OptionalAttribute(name: "idlocation", (dr, b) => b.SetIdLocation(dr.AsLocationExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "namelist", (dr, b) => b.SetNameList(dr.AsLocationExpressionList(dr.AttributeValue)))
		  .OptionalAttribute(name: "autoforward", (dr, b) => b.SetAutoForward(XmlConvert.ToBoolean(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "param", async (dr, b) => b.AddParam(await dr.ReadParam().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "finalize", async (dr, b) => b.SetFinalize(await dr.ReadFinalize().ConfigureAwait(false)))
		  .OptionalElement(Const.ScxmlNs, name: "content", async (dr, b) => b.SetContent(await dr.ReadContent().ConfigureAwait(false)));

	private async ValueTask<IFinalize> ReadFinalize() => (await Populate(FinalizeBuilderFactory(CreateAncestor()), FinalizePolicy).ConfigureAwait(false)).Build();

	private static void FinalizeBuildPolicy(IPolicyBuilder<IFinalizeBuilder> pb) =>
		pb.MultipleElements(Const.ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
		  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

	private async ValueTask<IScript> ReadScript() => (await Populate(ScriptBuilderFactory(CreateAncestor()), ScriptPolicy).ConfigureAwait(false)).Build();

	private static void ScriptBuildPolicy(IPolicyBuilder<IScriptBuilder> pb) =>
		pb.OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsExternalScriptExpression(dr.AttributeValue)))
		  .RawContent((dr, b) => b.SetBody(dr.AsScriptExpression(dr.RawContent)));

	private async ValueTask<IDataModel> ReadDataModel() => (await Populate(DataModelBuilderFactory(CreateAncestor()), DataModelPolicy).ConfigureAwait(false)).Build();

	private static void DataModelBuildPolicy(IPolicyBuilder<IDataModelBuilder> pb) =>
		pb.MultipleElements(Const.ScxmlNs, name: "data", async (dr, b) => b.AddData(await dr.ReadData().ConfigureAwait(false)));

	private async ValueTask<IData> ReadData() => (await Populate(DataBuilderFactory(CreateAncestor()), DataPolicy).ConfigureAwait(false)).Build();

	private static void DataBuildPolicy(IPolicyBuilder<IDataBuilder> pb) =>
		pb.RequiredAttribute(name: "id", (dr, b) => b.SetId(dr.AttributeValue))
		  .OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsExternalDataExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .RawContent((dr, b) => b.SetInlineContent(dr.AsInlineContent(dr.RawContent)));

	private async ValueTask<IDoneData> ReadDoneData() => (await Populate(DoneDataBuilderFactory(CreateAncestor()), DoneDataPolicy).ConfigureAwait(false)).Build();

	private static void DoneDataBuildPolicy(IPolicyBuilder<IDoneDataBuilder> pb) =>
		pb.OptionalElement(Const.ScxmlNs, name: "content", async (dr, b) => b.SetContent(await dr.ReadContent().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "param", async (dr, b) => b.AddParameter(await dr.ReadParam().ConfigureAwait(false)));

	private async ValueTask<IForEach> ReadForEach() => (await Populate(ForEachBuilderFactory(CreateAncestor()), ForEachPolicy).ConfigureAwait(false)).Build();

	private static void ForEachBuildPolicy(IPolicyBuilder<IForEachBuilder> pb) =>
		pb.RequiredAttribute(name: "array", (dr, b) => b.SetArray(dr.AsValueExpression(dr.AttributeValue)))
		  .RequiredAttribute(name: "item", (dr, b) => b.SetItem(dr.AsLocationExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "index", (dr, b) => b.SetIndex(dr.AsLocationExpression(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
		  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

	private async ValueTask<IIf> ReadIf() => (await Populate(IfBuilderFactory(CreateAncestor()), IfPolicy).ConfigureAwait(false)).Build();

	private static void IfBuildPolicy(IPolicyBuilder<IIfBuilder> pb) =>
		pb.RequiredAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.AttributeValue)))
		  .MultipleElements(Const.ScxmlNs, name: "elseif", async (dr, b) => b.AddAction(await dr.ReadElseIf().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "else", async (dr, b) => b.AddAction(await dr.ReadElse().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
		  .MultipleElements(Const.ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
		  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

	private async ValueTask<IElse> ReadElse() => (await Populate(ElseBuilderFactory(CreateAncestor()), ElsePolicy).ConfigureAwait(false)).Build();

	private static void ElseBuildPolicy(IPolicyBuilder<IElseBuilder> pb) { }

	private async ValueTask<IElseIf> ReadElseIf() => (await Populate(ElseIfBuilderFactory(CreateAncestor()), ElseIfPolicy).ConfigureAwait(false)).Build();

	private static void ElseIfBuildPolicy(IPolicyBuilder<IElseIfBuilder> pb) => pb.RequiredAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.AttributeValue)));

	private async ValueTask<IRaise> ReadRaise() => (await Populate(RaiseBuilderFactory(CreateAncestor()), RaisePolicy).ConfigureAwait(false)).Build();

	private static void RaiseBuildPolicy(IPolicyBuilder<IRaiseBuilder> pb) => pb.RequiredAttribute(name: "event", (dr, b) => b.SetEvent(AsEvent(dr.AttributeValue)));

	private async ValueTask<IAssign> ReadAssign() => (await Populate(AssignBuilderFactory(CreateAncestor()), AssignPolicy).ConfigureAwait(false)).Build();

	private static void AssignBuildPolicy(IPolicyBuilder<IAssignBuilder> pb) =>
		pb.RequiredAttribute(name: "location", (dr, b) => b.SetLocation(dr.AsLocationExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
		  .OptionalAttribute(name: "type", (dr, b) => b.SetType(dr.AttributeValue))
		  .OptionalAttribute(name: "attr", (dr, b) => b.SetAttribute(dr.AttributeValue))
		  .RawContent((dr, b) => b.SetInlineContent(dr.AsInlineContent(dr.RawContent)));

	private async ValueTask<ICancel> ReadCancel() => (await Populate(CancelBuilderFactory(CreateAncestor()), CancelPolicy).ConfigureAwait(false)).Build();

	private static void CancelBuildPolicy(IPolicyBuilder<ICancelBuilder> pb) =>
		pb.OptionalAttribute(name: "sendid", (dr, b) => b.SetSendId(dr.AttributeValue))
		  .OptionalAttribute(name: "sendidexpr", (dr, b) => b.SetSendIdExpression(dr.AsValueExpression(dr.AttributeValue)));

	private async ValueTask<ICustomAction> ReadCustomAction()
	{
		var builder = CustomActionBuilderFactory(CreateAncestor(namespaces: true));
		builder.SetXml(CurrentNamespace, CurrentName, await ReadOuterXml().ConfigureAwait(false));

		return builder.Build();
	}

	private class XmlNamespacesInfo(ImmutableArray<PrefixNamespace> namespaces, object? ancestor) : IXmlNamespacesInfo, IAncestorProvider
	{
	#region Interface IAncestorProvider

		public object? Ancestor { get; } = ancestor;

	#endregion

	#region Interface IXmlNamespacesInfo

		public ImmutableArray<PrefixNamespace> Namespaces { get; } = namespaces;

	#endregion
	}

	private class XmlLineInfo(int lineNumber, int linePosition, object? ancestor) : IXmlLineInfo, IAncestorProvider
	{
	#region Interface IAncestorProvider

		public object? Ancestor { get; } = ancestor;

	#endregion

	#region Interface IXmlLineInfo

		public bool HasLineInfo() => true;

		public int LineNumber { get; } = lineNumber;

		public int LinePosition { get; } = linePosition;

	#endregion
	}
}