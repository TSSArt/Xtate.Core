#region Copyright © 2019-2020 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Annotations;
using Xtate.Builder;
using Xtate.XInclude;

namespace Xtate.Scxml
{
	[PublicAPI]
	public class ScxmlDirector : XmlDirector<ScxmlDirector>
	{
		private const string ScxmlNs      = "http://www.w3.org/2005/07/scxml";
		private const string XtateScxmlNs = "http://xtate.net/scxml";
		private const char   Space        = ' ';

		private static readonly char[] SpaceSplitter = { Space };

		private static readonly Policy<IStateMachineBuilder> StateMachinePolicy = BuildPolicy<IStateMachineBuilder>(StateMachineBuildPolicy);
		private static readonly Policy<IStateBuilder>        StatePolicy        = BuildPolicy<IStateBuilder>(StateBuildPolicy);
		private static readonly Policy<IParallelBuilder>     ParallelPolicy     = BuildPolicy<IParallelBuilder>(ParallelBuildPolicy);
		private static readonly Policy<IFinalBuilder>        FinalPolicy        = BuildPolicy<IFinalBuilder>(FinalBuildPolicy);
		private static readonly Policy<IInitialBuilder>      InitialPolicy      = BuildPolicy<IInitialBuilder>(InitialBuildPolicy);
		private static readonly Policy<IHistoryBuilder>      HistoryPolicy      = BuildPolicy<IHistoryBuilder>(HistoryBuildPolicy);
		private static readonly Policy<ITransitionBuilder>   TransitionPolicy   = BuildPolicy<ITransitionBuilder>(TransitionBuildPolicy);
		private static readonly Policy<ILogBuilder>          LogPolicy          = BuildPolicy<ILogBuilder>(LogBuildPolicy);
		private static readonly Policy<ISendBuilder>         SendPolicy         = BuildPolicy<ISendBuilder>(SendBuildPolicy);
		private static readonly Policy<IParamBuilder>        ParamPolicy        = BuildPolicy<IParamBuilder>(ParamBuildPolicy);
		private static readonly Policy<IContentBuilder>      ContentPolicy      = BuildPolicy<IContentBuilder>(ContentBuildPolicy);
		private static readonly Policy<IOnEntryBuilder>      OnEntryPolicy      = BuildPolicy<IOnEntryBuilder>(OnEntryBuildPolicy);
		private static readonly Policy<IOnExitBuilder>       OnExitPolicy       = BuildPolicy<IOnExitBuilder>(OnExitBuildPolicy);
		private static readonly Policy<IInvokeBuilder>       InvokePolicy       = BuildPolicy<IInvokeBuilder>(InvokeBuildPolicy);
		private static readonly Policy<IFinalizeBuilder>     FinalizePolicy     = BuildPolicy<IFinalizeBuilder>(FinalizeBuildPolicy);
		private static readonly Policy<IScriptBuilder>       ScriptPolicy       = BuildPolicy<IScriptBuilder>(ScriptBuildPolicy);
		private static readonly Policy<IDataModelBuilder>    DataModelPolicy    = BuildPolicy<IDataModelBuilder>(DataModelBuildPolicy);
		private static readonly Policy<IDataBuilder>         DataPolicy         = BuildPolicy<IDataBuilder>(DataBuildPolicy);
		private static readonly Policy<IDoneDataBuilder>     DoneDataPolicy     = BuildPolicy<IDoneDataBuilder>(DoneDataBuildPolicy);
		private static readonly Policy<IForEachBuilder>      ForEachPolicy      = BuildPolicy<IForEachBuilder>(ForEachBuildPolicy);
		private static readonly Policy<IIfBuilder>           IfPolicy           = BuildPolicy<IIfBuilder>(IfBuildPolicy);
		private static readonly Policy<IElseBuilder>         ElsePolicy         = BuildPolicy<IElseBuilder>(ElseBuildPolicy);
		private static readonly Policy<IElseIfBuilder>       ElseIfPolicy       = BuildPolicy<IElseIfBuilder>(ElseIfBuildPolicy);
		private static readonly Policy<IRaiseBuilder>        RaisePolicy        = BuildPolicy<IRaiseBuilder>(RaiseBuildPolicy);
		private static readonly Policy<IAssignBuilder>       AssignPolicy       = BuildPolicy<IAssignBuilder>(AssignBuildPolicy);
		private static readonly Policy<ICancelBuilder>       CancelPolicy       = BuildPolicy<ICancelBuilder>(CancelBuildPolicy);

		private readonly IErrorProcessor                        _errorProcessor;
		private readonly IBuilderFactory                        _factory;
		private readonly IXmlNamespaceResolver?                 _namespaceResolver;
		private readonly XmlNameTable                           _nameTable;
		private readonly IStateMachineValidator?                _stateMachineValidator;
		private          List<ImmutableArray<PrefixNamespace>>? _nsCache;

		[SuppressMessage(category: "ReSharper", checkId: "ConstantNullCoalescingCondition")]
		public ScxmlDirector(XmlReader xmlReader, IBuilderFactory factory, ScxmlDirectorOptions? options = default)
				: base(GetReaderForXmlDirector(xmlReader, options), options?.ErrorProcessor ?? DefaultErrorProcessor.Instance, options?.Async ?? false)
		{
			_namespaceResolver = options?.NamespaceResolver;
			_stateMachineValidator = options?.StateMachineValidator;
			_factory = factory;
			_errorProcessor = options?.ErrorProcessor ?? DefaultErrorProcessor.Instance;
			_nameTable = xmlReader.NameTable ?? new NameTable();

			FillNameTable(_nameTable);
		}

		private static XmlReader GetReaderForXmlDirector(XmlReader xmlReader, ScxmlDirectorOptions? options)
		{
			if (xmlReader is null) throw new ArgumentNullException(nameof(xmlReader));

			var xmlResolver = options?.XmlResolver ?? ScxmlXmlResolver.DefaultInstance;
			var settings = options?.XmlReaderSettings is null ? new XmlReaderSettings() : options.XmlReaderSettings.Clone();
			settings.XmlResolver = xmlResolver;
			settings.NameTable = xmlReader.NameTable;
			settings.Async = true;

			return new XIncludeReader(xmlReader, settings, xmlResolver, options?.MaxNestingLevel ?? 0);
		}

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

		public async ValueTask<IStateMachine> ConstructStateMachine()
		{
			var stateMachine = await ReadStateMachine().ConfigureAwait(false);

			_stateMachineValidator?.Validate(stateMachine, _errorProcessor);

			return stateMachine;
		}

		private static IIdentifier AsIdentifier(string val)
		{
			if (val is null) throw new ArgumentNullException(nameof(val));

			return (Identifier) val;
		}

		private static IOutgoingEvent AsEvent(string val)
		{
			if (val is null) throw new ArgumentNullException(nameof(val));

			return new EventEntity(val) { Target = EventEntity.InternalTarget };
		}

		private static ImmutableArray<IIdentifier> AsIdentifierList(string val)
		{
			if (string.IsNullOrEmpty(val))
			{
				throw new ArgumentException(Resources.Exception_ListOfIdentifiersCannotBeEmpty, nameof(val));
			}

			if (val.IndexOf(Space) < 0)
			{
				return ImmutableArray.Create<IIdentifier>((Identifier) val);
			}

			var identifiers = val.Split(SpaceSplitter, StringSplitOptions.RemoveEmptyEntries);

			if (identifiers.Length == 0)
			{
				throw new ArgumentException(Resources.Exception_ListOfIdentifiersCannotBeEmpty, nameof(val));
			}

			var builder = ImmutableArray.CreateBuilder<IIdentifier>(identifiers.Length);

			foreach (var identifier in identifiers)
			{
				builder.Add((Identifier) identifier);
			}

			return builder.MoveToImmutable();
		}

		private static ImmutableArray<IEventDescriptor> AsEventDescriptorList(string val)
		{
			if (string.IsNullOrEmpty(val))
			{
				throw new ArgumentException(Resources.Exception_ListOfEventsCannotBeEmpty, nameof(val));
			}

			if (val.IndexOf(Space) < 0)
			{
				return ImmutableArray.Create<IEventDescriptor>((EventDescriptor) val);
			}

			var eventDescriptors = val.Split(SpaceSplitter, StringSplitOptions.RemoveEmptyEntries);

			if (eventDescriptors.Length == 0)
			{
				throw new ArgumentException(Resources.Exception_ListOfEventsCannotBeEmpty, nameof(val));
			}

			var builder = ImmutableArray.CreateBuilder<IEventDescriptor>(eventDescriptors.Length);

			foreach (var identifier in eventDescriptors)
			{
				builder.Add((EventDescriptor) identifier);
			}

			return builder.MoveToImmutable();
		}

		private IConditionExpression AsConditionalExpression(string expression)
		{
			if (string.IsNullOrEmpty(expression))
			{
				throw new ArgumentException(Resources.Exception_ConditionDoesNotSpecified, nameof(expression));
			}

			return new ConditionExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private ILocationExpression AsLocationExpression(string expression)
		{
			if (string.IsNullOrEmpty(expression))
			{
				throw new ArgumentException(Resources.Exception_LocationDoesNotSpecified, nameof(expression));
			}

			return new LocationExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private ImmutableArray<ILocationExpression> AsLocationExpressionList(string expression)
		{
			if (string.IsNullOrEmpty(expression))
			{
				throw new ArgumentException(Resources.Exception_ListOfLocationsCannotBeEmpty, nameof(expression));
			}

			if (expression.IndexOf(Space) < 0)
			{
				return ImmutableArray.Create<ILocationExpression>(new LocationExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) });
			}

			var locationExpressions = expression.Split(SpaceSplitter, StringSplitOptions.RemoveEmptyEntries);

			if (locationExpressions.Length == 0)
			{
				throw new ArgumentException(Resources.Exception_ListOfLocationsCannotBeEmpty, nameof(expression));
			}

			var builder = ImmutableArray.CreateBuilder<ILocationExpression>(locationExpressions.Length);

			foreach (var locationExpression in locationExpressions)
			{
				builder.Add(new LocationExpression { Expression = locationExpression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) });
			}

			return builder.MoveToImmutable();
		}

		private IValueExpression AsValueExpression(string expression)
		{
			if (expression is null) throw new ArgumentNullException(nameof(expression));

			return new ValueExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private IScriptExpression AsScriptExpression(string expression)
		{
			if (expression is null) throw new ArgumentNullException(nameof(expression));

			return new ScriptExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private IInlineContent AsInlineContent(string inlineContent)
		{
			if (inlineContent is null) throw new ArgumentNullException(nameof(inlineContent));

			return new InlineContent { Value = inlineContent, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private IContentBody AsContentBody(string contentBody)
		{
			if (contentBody is null) throw new ArgumentNullException(nameof(contentBody));

			return new ContentBody { Value = contentBody, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private static IExternalScriptExpression AsExternalScriptExpression(string uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return new ExternalScriptExpression { Uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
		}

		private static IExternalDataExpression AsExternalDataExpression(string uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return new ExternalDataExpression { Uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
		}

		private static Uri AsUri(string uri)
		{
			if (uri is null) throw new ArgumentNullException(nameof(uri));

			return new Uri(uri, UriKind.RelativeOrAbsolute);
		}

		private static T AsEnum<T>(string val) where T : struct
		{
			if (val is null) throw new ArgumentNullException(nameof(val));

			if (!Enum.TryParse(val, ignoreCase: true, out T result) || val.ToLowerInvariant() != val)
			{
				throw new ArgumentException(Res.Format(Resources.Exception_ValueCannotBeParsed, typeof(T).Name));
			}

			return result;
		}

		private static int AsMilliseconds(string val)
		{
			if (string.IsNullOrEmpty(val))
			{
				throw new ArgumentException(Resources.Exception_ValueCantBeEmpty, nameof(val));
			}

			if (val == @"0")
			{
				return 0;
			}

			const string ms = "ms";
			if (val.EndsWith(ms, StringComparison.Ordinal))
			{
				return int.Parse(val[..^ms.Length], NumberFormatInfo.InvariantInfo);
			}

			const string s = "s";
			if (val.EndsWith(s, StringComparison.Ordinal))
			{
				return int.Parse(val[..^s.Length], NumberFormatInfo.InvariantInfo) * 1000;
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

		private object? CreateAncestor(bool namespaces = false, bool nameTable = false)
		{
			var result = CreateXmlLineInfo(null);

			if (namespaces)
			{
				result = CreateXmlNamespacesInfo(result);
			}

			if (nameTable)
			{
				result = CreateNameTableInfo(result);
			}

			return result;
		}

		private object? CreateXmlLineInfo(object? ancestor) => _errorProcessor.LineInfoRequired && HasLineInfo() ? new XmlLineInfo(LineNumber, LinePosition, ancestor) : ancestor;

		private object CreateNameTableInfo(object? ancestor) => new AncestorContainer(_nameTable, ancestor);

		private object? CreateXmlNamespacesInfo(object? ancestor)
		{
			var namespaces = _namespaceResolver?.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

			if (namespaces is null || namespaces.Count == 0 || namespaces.Count == 1 && namespaces.ContainsKey(string.Empty))
			{
				return ancestor;
			}

			namespaces.Remove(string.Empty);

			var array = ResolveThroughCache(namespaces);

			if (array.IsDefaultOrEmpty)
			{
				return ancestor;
			}

			return new XmlNamespacesInfo(array, ancestor);
		}

		private ImmutableArray<PrefixNamespace> ResolveThroughCache(ICollection<KeyValuePair<string, string>> list)
		{
			_nsCache ??= new List<ImmutableArray<PrefixNamespace>>();

			foreach (var item in _nsCache)
			{
				if (CompareArrays(item, list))
				{
					return item;
				}
			}

			var builder = ImmutableArray.CreateBuilder<PrefixNamespace>(list.Count);

			foreach (var pair in list)
			{
				builder.Add(new PrefixNamespace(pair.Key, pair.Value));
			}

			var array = builder.MoveToImmutable();

			_nsCache.Add(array);

			return array;
		}

		private static bool CompareArrays(ImmutableArray<PrefixNamespace> array, ICollection<KeyValuePair<string, string>> list)
		{
			if (array.Length != list.Count)
			{
				return false;
			}

			var index = 0;

			foreach (var pair in list)
			{
				if (!ReferenceEquals(pair.Key, array[index].Prefix) || !ReferenceEquals(pair.Value, array[index].Namespace))
				{
					return false;
				}

				index ++;
			}

			return true;
		}

		private async ValueTask<IStateMachine> ReadStateMachine() => (await Populate(_factory.CreateStateMachineBuilder(CreateAncestor()), StateMachinePolicy).ConfigureAwait(false)).Build();

		private static void StateMachineBuildPolicy(IPolicyBuilder<IStateMachineBuilder> pb) =>
				pb.ValidateElementName(ScxmlNs, name: "scxml")
				  .RequiredAttribute(name: "version", (dr, _) => CheckScxmlVersion(dr.AttributeValue))
				  .OptionalAttribute(name: "initial", (dr, b) => b.SetInitial(AsIdentifierList(dr.AttributeValue)))
				  .OptionalAttribute(name: "datamodel", (dr, b) => b.SetDataModelType(dr.AttributeValue))
				  .OptionalAttribute(name: "binding", (dr, b) => b.SetBindingType(AsEnum<BindingType>(dr.AttributeValue)))
				  .OptionalAttribute(name: "name", (dr, b) => b.SetName(dr.AttributeValue))
				  .MultipleElements(ScxmlNs, name: "state", async (dr, b) => b.AddState(await dr.ReadState().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "parallel", async (dr, b) => b.AddParallel(await dr.ReadParallel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "final", async (dr, b) => b.AddFinal(await dr.ReadFinal().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "datamodel", async (dr, b) => b.SetDataModel(await dr.ReadDataModel().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "script", async (dr, b) => b.SetScript(await dr.ReadScript().ConfigureAwait(false)))
				  .OptionalAttribute(XtateScxmlNs, name: "synchronous", (dr, b) => b.SetSynchronousEventProcessing(XmlConvert.ToBoolean(dr.AttributeValue)))
				  .OptionalAttribute(XtateScxmlNs, name: "queueSize", (dr, b) => b.SetExternalQueueSize(XmlConvert.ToInt32(dr.AttributeValue)))
				  .OptionalAttribute(XtateScxmlNs, name: "persistence", (dr, b) => b.SetPersistenceLevel((PersistenceLevel) Enum.Parse(typeof(PersistenceLevel), dr.AttributeValue)))
				  .OptionalAttribute(XtateScxmlNs, name: "onError", (dr, b) => b.SetUnhandledErrorBehaviour((UnhandledErrorBehaviour) Enum.Parse(typeof(UnhandledErrorBehaviour), dr.AttributeValue)));

		private async ValueTask<IState> ReadState() => (await Populate(_factory.CreateStateBuilder(CreateAncestor()), StatePolicy).ConfigureAwait(false)).Build();

		private static void StateBuildPolicy(IPolicyBuilder<IStateBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
				  .OptionalAttribute(name: "initial", (dr, b) => b.SetInitial(AsIdentifierList(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "state", async (dr, b) => b.AddState(await dr.ReadState().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "parallel", async (dr, b) => b.AddParallel(await dr.ReadParallel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "final", async (dr, b) => b.AddFinal(await dr.ReadFinal().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "history", async (dr, b) => b.AddHistory(await dr.ReadHistory().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "invoke", async (dr, b) => b.AddInvoke(await dr.ReadInvoke().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "transition", async (dr, b) => b.AddTransition(await dr.ReadTransition().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "onentry", async (dr, b) => b.AddOnEntry(await dr.ReadOnEntry().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "onexit", async (dr, b) => b.AddOnExit(await dr.ReadOnExit().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "initial", async (dr, b) => b.SetInitial(await dr.ReadInitial().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "datamodel", async (dr, b) => b.SetDataModel(await dr.ReadDataModel().ConfigureAwait(false)));

		private async ValueTask<IParallel> ReadParallel() => (await Populate(_factory.CreateParallelBuilder(CreateAncestor()), ParallelPolicy).ConfigureAwait(false)).Build();

		private static void ParallelBuildPolicy(IPolicyBuilder<IParallelBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "state", async (dr, b) => b.AddState(await dr.ReadState().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "parallel", async (dr, b) => b.AddParallel(await dr.ReadParallel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "history", async (dr, b) => b.AddHistory(await dr.ReadHistory().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "invoke", async (dr, b) => b.AddInvoke(await dr.ReadInvoke().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "transition", async (dr, b) => b.AddTransition(await dr.ReadTransition().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "onentry", async (dr, b) => b.AddOnEntry(await dr.ReadOnEntry().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "onexit", async (dr, b) => b.AddOnExit(await dr.ReadOnExit().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "datamodel", async (dr, b) => b.SetDataModel(await dr.ReadDataModel().ConfigureAwait(false)));

		private async ValueTask<IFinal> ReadFinal() => (await Populate(_factory.CreateFinalBuilder(CreateAncestor()), FinalPolicy).ConfigureAwait(false)).Build();

		private static void FinalBuildPolicy(IPolicyBuilder<IFinalBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "onentry", async (dr, b) => b.AddOnEntry(await dr.ReadOnEntry().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "onexit", async (dr, b) => b.AddOnExit(await dr.ReadOnExit().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "donedata", async (dr, b) => b.SetDoneData(await dr.ReadDoneData().ConfigureAwait(false)));

		private async ValueTask<IInitial> ReadInitial() => (await Populate(_factory.CreateInitialBuilder(CreateAncestor()), InitialPolicy).ConfigureAwait(false)).Build();

		private static void InitialBuildPolicy(IPolicyBuilder<IInitialBuilder> pb) =>
				pb.SingleElement(ScxmlNs, name: "transition", async (dr, b) => b.SetTransition(await dr.ReadTransition().ConfigureAwait(false)));

		private async ValueTask<IHistory> ReadHistory() => (await Populate(_factory.CreateHistoryBuilder(CreateAncestor()), HistoryPolicy).ConfigureAwait(false)).Build();

		private static void HistoryBuildPolicy(IPolicyBuilder<IHistoryBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.AttributeValue)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsEnum<HistoryType>(dr.AttributeValue)))
				  .SingleElement(ScxmlNs, name: "transition", async (dr, b) => b.SetTransition(await dr.ReadTransition().ConfigureAwait(false)));

		private async ValueTask<ITransition> ReadTransition() => (await Populate(_factory.CreateTransitionBuilder(CreateAncestor()), TransitionPolicy).ConfigureAwait(false)).Build();

		private static void TransitionBuildPolicy(IPolicyBuilder<ITransitionBuilder> pb) =>
				pb.OptionalAttribute(name: "event", (dr, b) => b.SetEvent(AsEventDescriptorList(dr.AttributeValue)))
				  .OptionalAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "target", (dr, b) => b.SetTarget(AsIdentifierList(dr.AttributeValue)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsEnum<TransitionType>(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
				  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

		private async ValueTask<ILog> ReadLog() => (await Populate(_factory.CreateLogBuilder(CreateAncestor()), LogPolicy).ConfigureAwait(false)).Build();

		private static void LogBuildPolicy(IPolicyBuilder<ILogBuilder> pb) =>
				pb.OptionalAttribute(name: "label", (dr, b) => b.SetLabel(dr.AttributeValue))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)));

		private async ValueTask<ISend> ReadSend() => (await Populate(_factory.CreateSendBuilder(CreateAncestor()), SendPolicy).ConfigureAwait(false)).Build();

		private static void SendBuildPolicy(IPolicyBuilder<ISendBuilder> pb) =>
				pb.OptionalAttribute(name: "event", (dr, b) => b.SetEvent(dr.AttributeValue))
				  .OptionalAttribute(name: "eventexpr", (dr, b) => b.SetEventExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "target", (dr, b) => b.SetTarget(AsUri(dr.AttributeValue)))
				  .OptionalAttribute(name: "targetexpr", (dr, b) => b.SetTargetExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsUri(dr.AttributeValue)))
				  .OptionalAttribute(name: "typeexpr", (dr, b) => b.SetTypeExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "id", (dr, b) => b.SetId(dr.AttributeValue))
				  .OptionalAttribute(name: "idlocation", (dr, b) => b.SetIdLocation(dr.AsLocationExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "delay", (dr, b) => b.SetDelay(AsMilliseconds(dr.AttributeValue)))
				  .OptionalAttribute(name: "delayexpr", (dr, b) => b.SetDelayExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "namelist", (dr, b) => b.SetNameList(dr.AsLocationExpressionList(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "param", async (dr, b) => b.AddParameter(await dr.ReadParam().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "content", async (dr, b) => b.SetContent(await dr.ReadContent().ConfigureAwait(false)));

		private async ValueTask<IParam> ReadParam() => (await Populate(_factory.CreateParamBuilder(CreateAncestor()), ParamPolicy).ConfigureAwait(false)).Build();

		private static void ParamBuildPolicy(IPolicyBuilder<IParamBuilder> pb) =>
				pb.RequiredAttribute(name: "name", (dr, b) => b.SetName(dr.AttributeValue))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "location", (dr, b) => b.SetLocation(dr.AsLocationExpression(dr.AttributeValue)));

		private async ValueTask<IContent> ReadContent() => (await Populate(_factory.CreateContentBuilder(CreateAncestor()), ContentPolicy).ConfigureAwait(false)).Build();

		private static void ContentBuildPolicy(IPolicyBuilder<IContentBuilder> pb) =>
				pb.OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .RawContent((dr, b) => b.SetBody(dr.AsContentBody(dr.RawContent)));

		private async ValueTask<IOnEntry> ReadOnEntry() => (await Populate(_factory.CreateOnEntryBuilder(CreateAncestor()), OnEntryPolicy).ConfigureAwait(false)).Build();

		private static void OnEntryBuildPolicy(IPolicyBuilder<IOnEntryBuilder> pb) =>
				pb.MultipleElements(ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
				  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

		private async ValueTask<IOnExit> ReadOnExit() => (await Populate(_factory.CreateOnExitBuilder(CreateAncestor()), OnExitPolicy).ConfigureAwait(false)).Build();

		private static void OnExitBuildPolicy(IPolicyBuilder<IOnExitBuilder> pb) =>
				pb.MultipleElements(ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
				  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

		private async ValueTask<IInvoke> ReadInvoke() => (await Populate(_factory.CreateInvokeBuilder(CreateAncestor()), InvokePolicy).ConfigureAwait(false)).Build();

		private static void InvokeBuildPolicy(IPolicyBuilder<IInvokeBuilder> pb) =>
				pb.OptionalAttribute(name: "type", (dr, b) => b.SetType(AsUri(dr.AttributeValue)))
				  .OptionalAttribute(name: "typeexpr", (dr, b) => b.SetTypeExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsUri(dr.AttributeValue)))
				  .OptionalAttribute(name: "srcexpr", (dr, b) => b.SetSourceExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "id", (dr, b) => b.SetId(dr.AttributeValue))
				  .OptionalAttribute(name: "idlocation", (dr, b) => b.SetIdLocation(dr.AsLocationExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "namelist", (dr, b) => b.SetNameList(dr.AsLocationExpressionList(dr.AttributeValue)))
				  .OptionalAttribute(name: "autoforward", (dr, b) => b.SetAutoForward(XmlConvert.ToBoolean(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "param", async (dr, b) => b.AddParam(await dr.ReadParam().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "finalize", async (dr, b) => b.SetFinalize(await dr.ReadFinalize().ConfigureAwait(false)))
				  .OptionalElement(ScxmlNs, name: "content", async (dr, b) => b.SetContent(await dr.ReadContent().ConfigureAwait(false)));

		private async ValueTask<IFinalize> ReadFinalize() => (await Populate(_factory.CreateFinalizeBuilder(CreateAncestor()), FinalizePolicy).ConfigureAwait(false)).Build();

		private static void FinalizeBuildPolicy(IPolicyBuilder<IFinalizeBuilder> pb) =>
				pb.MultipleElements(ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
				  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

		private async ValueTask<IScript> ReadScript() => (await Populate(_factory.CreateScriptBuilder(CreateAncestor()), ScriptPolicy).ConfigureAwait(false)).Build();

		private static void ScriptBuildPolicy(IPolicyBuilder<IScriptBuilder> pb) =>
				pb.OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsExternalScriptExpression(dr.AttributeValue)))
				  .RawContent((dr, b) => b.SetBody(dr.AsScriptExpression(dr.RawContent)));

		private async ValueTask<IDataModel> ReadDataModel() => (await Populate(_factory.CreateDataModelBuilder(CreateAncestor()), DataModelPolicy).ConfigureAwait(false)).Build();

		private static void DataModelBuildPolicy(IPolicyBuilder<IDataModelBuilder> pb) =>
				pb.MultipleElements(ScxmlNs, name: "data", async (dr, b) => b.AddData(await dr.ReadData().ConfigureAwait(false)));

		private async ValueTask<IData> ReadData() => (await Populate(_factory.CreateDataBuilder(CreateAncestor()), DataPolicy).ConfigureAwait(false)).Build();

		private static void DataBuildPolicy(IPolicyBuilder<IDataBuilder> pb) =>
				pb.RequiredAttribute(name: "id", (dr, b) => b.SetId(dr.AttributeValue))
				  .OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsExternalDataExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .RawContent((dr, b) => b.SetInlineContent(dr.AsInlineContent(dr.RawContent)));

		private async ValueTask<IDoneData> ReadDoneData() => (await Populate(_factory.CreateDoneDataBuilder(CreateAncestor()), DoneDataPolicy).ConfigureAwait(false)).Build();

		private static void DoneDataBuildPolicy(IPolicyBuilder<IDoneDataBuilder> pb) =>
				pb.OptionalElement(ScxmlNs, name: "content", async (dr, b) => b.SetContent(await dr.ReadContent().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "param", async (dr, b) => b.AddParameter(await dr.ReadParam().ConfigureAwait(false)));

		private async ValueTask<IForEach> ReadForEach() => (await Populate(_factory.CreateForEachBuilder(CreateAncestor()), ForEachPolicy).ConfigureAwait(false)).Build();

		private static void ForEachBuildPolicy(IPolicyBuilder<IForEachBuilder> pb) =>
				pb.RequiredAttribute(name: "array", (dr, b) => b.SetArray(dr.AsValueExpression(dr.AttributeValue)))
				  .RequiredAttribute(name: "item", (dr, b) => b.SetItem(dr.AsLocationExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "index", (dr, b) => b.SetIndex(dr.AsLocationExpression(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
				  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

		private async ValueTask<IIf> ReadIf() => (await Populate(_factory.CreateIfBuilder(CreateAncestor()), IfPolicy).ConfigureAwait(false)).Build();

		private static void IfBuildPolicy(IPolicyBuilder<IIfBuilder> pb) =>
				pb.RequiredAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.AttributeValue)))
				  .MultipleElements(ScxmlNs, name: "elseif", async (dr, b) => b.AddAction(await dr.ReadElseIf().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "else", async (dr, b) => b.AddAction(await dr.ReadElse().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "assign", async (dr, b) => b.AddAction(await dr.ReadAssign().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "foreach", async (dr, b) => b.AddAction(await dr.ReadForEach().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "if", async (dr, b) => b.AddAction(await dr.ReadIf().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "log", async (dr, b) => b.AddAction(await dr.ReadLog().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "raise", async (dr, b) => b.AddAction(await dr.ReadRaise().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "send", async (dr, b) => b.AddAction(await dr.ReadSend().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "cancel", async (dr, b) => b.AddAction(await dr.ReadCancel().ConfigureAwait(false)))
				  .MultipleElements(ScxmlNs, name: "script", async (dr, b) => b.AddAction(await dr.ReadScript().ConfigureAwait(false)))
				  .UnknownElement(async (dr, b) => b.AddAction(await dr.ReadCustomAction().ConfigureAwait(false)));

		private async ValueTask<IElse> ReadElse() => (await Populate(_factory.CreateElseBuilder(CreateAncestor()), ElsePolicy).ConfigureAwait(false)).Build();

		private static void ElseBuildPolicy(IPolicyBuilder<IElseBuilder> pb) { }

		private async ValueTask<IElseIf> ReadElseIf() => (await Populate(_factory.CreateElseIfBuilder(CreateAncestor()), ElseIfPolicy).ConfigureAwait(false)).Build();

		private static void ElseIfBuildPolicy(IPolicyBuilder<IElseIfBuilder> pb) => pb.RequiredAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.AttributeValue)));

		private async ValueTask<IRaise> ReadRaise() => (await Populate(_factory.CreateRaiseBuilder(CreateAncestor()), RaisePolicy).ConfigureAwait(false)).Build();

		private static void RaiseBuildPolicy(IPolicyBuilder<IRaiseBuilder> pb) => pb.RequiredAttribute(name: "event", (dr, b) => b.SetEvent(AsEvent(dr.AttributeValue)));

		private async ValueTask<IAssign> ReadAssign() => (await Populate(_factory.CreateAssignBuilder(CreateAncestor()), AssignPolicy).ConfigureAwait(false)).Build();

		private static void AssignBuildPolicy(IPolicyBuilder<IAssignBuilder> pb) =>
				pb.RequiredAttribute(name: "location", (dr, b) => b.SetLocation(dr.AsLocationExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.AttributeValue)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(dr.AttributeValue))
				  .OptionalAttribute(name: "attr", (dr, b) => b.SetAttribute(dr.AttributeValue))
				  .RawContent((dr, b) => b.SetInlineContent(dr.AsInlineContent(dr.RawContent)));

		private async ValueTask<ICancel> ReadCancel() => (await Populate(_factory.CreateCancelBuilder(CreateAncestor()), CancelPolicy).ConfigureAwait(false)).Build();

		private static void CancelBuildPolicy(IPolicyBuilder<ICancelBuilder> pb) =>
				pb.OptionalAttribute(name: "sendid", (dr, b) => b.SetSendId(dr.AttributeValue))
				  .OptionalAttribute(name: "sendidexpr", (dr, b) => b.SetSendIdExpression(dr.AsValueExpression(dr.AttributeValue)));

		private async ValueTask<ICustomAction> ReadCustomAction()
		{
			var builder = _factory.CreateCustomActionBuilder(CreateAncestor(namespaces: true, nameTable: true));
			builder.SetXml(CurrentNamespace, CurrentName, await ReadOuterXml().ConfigureAwait(false));
			return builder.Build();
		}

		private class XmlNamespacesInfo : IXmlNamespacesInfo, IAncestorProvider
		{
			public XmlNamespacesInfo(ImmutableArray<PrefixNamespace> namespaces, object? ancestor)
			{
				Namespaces = namespaces;
				Ancestor = ancestor;
			}

		#region Interface IAncestorProvider

			public object? Ancestor { get; }

		#endregion

		#region Interface IXmlNamespacesInfo

			public ImmutableArray<PrefixNamespace> Namespaces { get; }

		#endregion
		}

		private class XmlLineInfo : IXmlLineInfo, IAncestorProvider
		{
			public XmlLineInfo(int lineNumber, int linePosition, object? ancestor)
			{
				LineNumber = lineNumber;
				LinePosition = linePosition;
				Ancestor = ancestor;
			}

		#region Interface IAncestorProvider

			public object? Ancestor { get; }

		#endregion

		#region Interface IXmlLineInfo

			public bool HasLineInfo() => true;

			public int LineNumber   { get; }
			public int LinePosition { get; }

		#endregion
		}
	}
}