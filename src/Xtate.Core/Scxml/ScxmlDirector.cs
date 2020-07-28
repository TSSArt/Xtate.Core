#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Xml;
using Xtate.Annotations;
using Xtate.Builder;

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
		private          List<ImmutableArray<PrefixNamespace>>? _nsCache;

		public ScxmlDirector(XmlReader xmlReader, IBuilderFactory factory, IErrorProcessor errorProcessor, IXmlNamespaceResolver? namespaceResolver) : base(xmlReader, errorProcessor)
		{
			_namespaceResolver = namespaceResolver;
			_factory = factory;
			_errorProcessor = errorProcessor;
			_nameTable = xmlReader.NameTable;

			FillNameTable(_nameTable);
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

		public IStateMachine ConstructStateMachine(IStateMachineValidator? stateMachineValidator = default)
		{
			var stateMachine = ReadStateMachine();

			stateMachineValidator?.Validate(stateMachine, _errorProcessor);

			return stateMachine;
		}

		private static IIdentifier AsIdentifier(string val)
		{
			if (val == null) throw new ArgumentNullException(nameof(val));

			return (Identifier) val;
		}

		private static IOutgoingEvent AsEvent(string val)
		{
			if (val == null) throw new ArgumentNullException(nameof(val));

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
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			return new ValueExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private IScriptExpression AsScriptExpression(string expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			return new ScriptExpression { Expression = expression, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private IInlineContent AsInlineContent(string inlineContent)
		{
			if (inlineContent == null) throw new ArgumentNullException(nameof(inlineContent));

			return new InlineContent { Value = inlineContent, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private IContentBody AsContentBody(string contentBody)
		{
			if (contentBody == null) throw new ArgumentNullException(nameof(contentBody));

			return new ContentBody { Value = contentBody, Ancestor = CreateAncestor(namespaces: true, nameTable: true) };
		}

		private static IExternalScriptExpression AsExternalScriptExpression(string uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			return new ExternalScriptExpression { Uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
		}

		private static IExternalDataExpression AsExternalDataExpression(string uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			return new ExternalDataExpression { Uri = new Uri(uri, UriKind.RelativeOrAbsolute) };
		}

		private static Uri AsUri(string uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			return new Uri(uri, UriKind.RelativeOrAbsolute);
		}

		private static T AsEnum<T>(string val) where T : struct
		{
			if (val == null) throw new ArgumentNullException(nameof(val));

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
				return int.Parse(val.Substring(startIndex: 0, val.Length - ms.Length), NumberFormatInfo.InvariantInfo);
			}

			const string s = "s";
			if (val.EndsWith(s, StringComparison.Ordinal))
			{
				return int.Parse(val.Substring(startIndex: 0, val.Length - s.Length), NumberFormatInfo.InvariantInfo) * 1000;
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

		private object? CreateNameTableInfo(object? ancestor) => new AncestorContainer(_nameTable, ancestor);

		private object? CreateXmlNamespacesInfo(object? ancestor)
		{
			var namespaces = _namespaceResolver?.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

			if (namespaces == null || namespaces.Count == 0 || namespaces.Count == 1 && namespaces.ContainsKey(string.Empty))
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

		private IStateMachine ReadStateMachine() => Populate(_factory.CreateStateMachineBuilder(CreateAncestor()), StateMachinePolicy).Build();

		private static void StateMachineBuildPolicy(IPolicyBuilder<IStateMachineBuilder> pb) =>
				pb.ValidateElementName(ScxmlNs, name: "scxml")
				  .RequiredAttribute(name: "version", (dr, b) => CheckScxmlVersion(dr.Current))
				  .OptionalAttribute(name: "initial", (dr, b) => b.SetInitial(AsIdentifierList(dr.Current)))
				  .OptionalAttribute(name: "datamodel", (dr, b) => b.SetDataModelType(dr.Current))
				  .OptionalAttribute(name: "binding", (dr, b) => b.SetBindingType(AsEnum<BindingType>(dr.Current)))
				  .OptionalAttribute(name: "name", (dr, b) => b.SetName(dr.Current))
				  .MultipleElements(ScxmlNs, name: "state", (dr, b) => b.AddState(dr.ReadState()))
				  .MultipleElements(ScxmlNs, name: "parallel", (dr, b) => b.AddParallel(dr.ReadParallel()))
				  .MultipleElements(ScxmlNs, name: "final", (dr, b) => b.AddFinal(dr.ReadFinal()))
				  .OptionalElement(ScxmlNs, name: "datamodel", (dr, b) => b.SetDataModel(dr.ReadDataModel()))
				  .OptionalElement(ScxmlNs, name: "script", (dr, b) => b.SetScript(dr.ReadScript()))
				  .OptionalAttribute(XtateScxmlNs, name: "synchronous", (dr, b) => b.SetSynchronousEventProcessing(XmlConvert.ToBoolean(dr.Current)))
				  .OptionalAttribute(XtateScxmlNs, name: "queueSize", (dr, b) => b.SetExternalQueueSize(XmlConvert.ToInt32(dr.Current)))
				  .OptionalAttribute(XtateScxmlNs, name: "persistence", (dr, b) => b.SetPersistenceLevel((PersistenceLevel) Enum.Parse(typeof(PersistenceLevel), dr.Current)))
				  .OptionalAttribute(XtateScxmlNs, name: "onError", (dr, b) => b.SetUnhandledErrorBehaviour((UnhandledErrorBehaviour) Enum.Parse(typeof(UnhandledErrorBehaviour), dr.Current)));

		private IState ReadState() => Populate(_factory.CreateStateBuilder(CreateAncestor()), StatePolicy).Build();

		private static void StateBuildPolicy(IPolicyBuilder<IStateBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.Current)))
				  .OptionalAttribute(name: "initial", (dr, b) => b.SetInitial(AsIdentifierList(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "state", (dr, b) => b.AddState(dr.ReadState()))
				  .MultipleElements(ScxmlNs, name: "parallel", (dr, b) => b.AddParallel(dr.ReadParallel()))
				  .MultipleElements(ScxmlNs, name: "final", (dr, b) => b.AddFinal(dr.ReadFinal()))
				  .MultipleElements(ScxmlNs, name: "history", (dr, b) => b.AddHistory(dr.ReadHistory()))
				  .MultipleElements(ScxmlNs, name: "invoke", (dr, b) => b.AddInvoke(dr.ReadInvoke()))
				  .MultipleElements(ScxmlNs, name: "transition", (dr, b) => b.AddTransition(dr.ReadTransition()))
				  .MultipleElements(ScxmlNs, name: "onentry", (dr, b) => b.AddOnEntry(dr.ReadOnEntry()))
				  .MultipleElements(ScxmlNs, name: "onexit", (dr, b) => b.AddOnExit(dr.ReadOnExit()))
				  .OptionalElement(ScxmlNs, name: "initial", (dr, b) => b.SetInitial(dr.ReadInitial()))
				  .OptionalElement(ScxmlNs, name: "datamodel", (dr, b) => b.SetDataModel(dr.ReadDataModel()));

		private IParallel ReadParallel() => Populate(_factory.CreateParallelBuilder(CreateAncestor()), ParallelPolicy).Build();

		private static void ParallelBuildPolicy(IPolicyBuilder<IParallelBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "state", (dr, b) => b.AddState(dr.ReadState()))
				  .MultipleElements(ScxmlNs, name: "parallel", (dr, b) => b.AddParallel(dr.ReadParallel()))
				  .MultipleElements(ScxmlNs, name: "history", (dr, b) => b.AddHistory(dr.ReadHistory()))
				  .MultipleElements(ScxmlNs, name: "invoke", (dr, b) => b.AddInvoke(dr.ReadInvoke()))
				  .MultipleElements(ScxmlNs, name: "transition", (dr, b) => b.AddTransition(dr.ReadTransition()))
				  .MultipleElements(ScxmlNs, name: "onentry", (dr, b) => b.AddOnEntry(dr.ReadOnEntry()))
				  .MultipleElements(ScxmlNs, name: "onexit", (dr, b) => b.AddOnExit(dr.ReadOnExit()))
				  .OptionalElement(ScxmlNs, name: "datamodel", (dr, b) => b.SetDataModel(dr.ReadDataModel()));

		private IFinal ReadFinal() => Populate(_factory.CreateFinalBuilder(CreateAncestor()), FinalPolicy).Build();

		private static void FinalBuildPolicy(IPolicyBuilder<IFinalBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "onentry", (dr, b) => b.AddOnEntry(dr.ReadOnEntry()))
				  .MultipleElements(ScxmlNs, name: "onexit", (dr, b) => b.AddOnExit(dr.ReadOnExit()))
				  .OptionalElement(ScxmlNs, name: "donedata", (dr, b) => b.SetDoneData(dr.ReadDoneData()));

		private IInitial ReadInitial() => Populate(_factory.CreateInitialBuilder(CreateAncestor()), InitialPolicy).Build();

		private static void InitialBuildPolicy(IPolicyBuilder<IInitialBuilder> pb) => pb.SingleElement(ScxmlNs, name: "transition", (dr, b) => b.SetTransition(dr.ReadTransition()));

		private IHistory ReadHistory() => Populate(_factory.CreateHistoryBuilder(CreateAncestor()), HistoryPolicy).Build();

		private static void HistoryBuildPolicy(IPolicyBuilder<IHistoryBuilder> pb) =>
				pb.OptionalAttribute(name: "id", (dr, b) => b.SetId(AsIdentifier(dr.Current)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsEnum<HistoryType>(dr.Current)))
				  .SingleElement(ScxmlNs, name: "transition", (dr, b) => b.SetTransition(dr.ReadTransition()));

		private ITransition ReadTransition() => Populate(_factory.CreateTransitionBuilder(CreateAncestor()), TransitionPolicy).Build();

		private static void TransitionBuildPolicy(IPolicyBuilder<ITransitionBuilder> pb) =>
				pb.OptionalAttribute(name: "event", (dr, b) => b.SetEvent(AsEventDescriptorList(dr.Current)))
				  .OptionalAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.Current)))
				  .OptionalAttribute(name: "target", (dr, b) => b.SetTarget(AsIdentifierList(dr.Current)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsEnum<TransitionType>(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "assign", (dr, b) => b.AddAction(dr.ReadAssign()))
				  .MultipleElements(ScxmlNs, name: "foreach", (dr, b) => b.AddAction(dr.ReadForEach()))
				  .MultipleElements(ScxmlNs, name: "if", (dr, b) => b.AddAction(dr.ReadIf()))
				  .MultipleElements(ScxmlNs, name: "log", (dr, b) => b.AddAction(dr.ReadLog()))
				  .MultipleElements(ScxmlNs, name: "raise", (dr, b) => b.AddAction(dr.ReadRaise()))
				  .MultipleElements(ScxmlNs, name: "send", (dr, b) => b.AddAction(dr.ReadSend()))
				  .MultipleElements(ScxmlNs, name: "cancel", (dr, b) => b.AddAction(dr.ReadCancel()))
				  .MultipleElements(ScxmlNs, name: "script", (dr, b) => b.AddAction(dr.ReadScript()))
				  .UnknownElement((dr, b) => b.AddAction(dr.ReadCustomAction()));

		private ILog ReadLog() => Populate(_factory.CreateLogBuilder(CreateAncestor()), LogPolicy).Build();

		private static void LogBuildPolicy(IPolicyBuilder<ILogBuilder> pb) =>
				pb.OptionalAttribute(name: "label", (dr, b) => b.SetLabel(dr.Current))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.Current)));

		private ISend ReadSend() => Populate(_factory.CreateSendBuilder(CreateAncestor()), SendPolicy).Build();

		private static void SendBuildPolicy(IPolicyBuilder<ISendBuilder> pb) =>
				pb.OptionalAttribute(name: "event", (dr, b) => b.SetEvent(dr.Current))
				  .OptionalAttribute(name: "eventexpr", (dr, b) => b.SetEventExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "target", (dr, b) => b.SetTarget(AsUri(dr.Current)))
				  .OptionalAttribute(name: "targetexpr", (dr, b) => b.SetTargetExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(AsUri(dr.Current)))
				  .OptionalAttribute(name: "typeexpr", (dr, b) => b.SetTypeExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "id", (dr, b) => b.SetId(dr.Current))
				  .OptionalAttribute(name: "idlocation", (dr, b) => b.SetIdLocation(dr.AsLocationExpression(dr.Current)))
				  .OptionalAttribute(name: "delay", (dr, b) => b.SetDelay(AsMilliseconds(dr.Current)))
				  .OptionalAttribute(name: "delayexpr", (dr, b) => b.SetDelayExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "namelist", (dr, b) => b.SetNameList(dr.AsLocationExpressionList(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "param", (dr, b) => b.AddParameter(dr.ReadParam()))
				  .OptionalElement(ScxmlNs, name: "content", (dr, b) => b.SetContent(dr.ReadContent()));

		private IParam ReadParam() => Populate(_factory.CreateParamBuilder(CreateAncestor()), ParamPolicy).Build();

		private static void ParamBuildPolicy(IPolicyBuilder<IParamBuilder> pb) =>
				pb.RequiredAttribute(name: "name", (dr, b) => b.SetName(dr.Current))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "location", (dr, b) => b.SetLocation(dr.AsLocationExpression(dr.Current)));

		private IContent ReadContent() => Populate(_factory.CreateContentBuilder(CreateAncestor()), ContentPolicy).Build();

		private static void ContentBuildPolicy(IPolicyBuilder<IContentBuilder> pb) =>
				pb.OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.Current)))
				  .RawContent((dr, b) => b.SetBody(dr.AsContentBody(dr.Current)));

		private IOnEntry ReadOnEntry() => Populate(_factory.CreateOnEntryBuilder(CreateAncestor()), OnEntryPolicy).Build();

		private static void OnEntryBuildPolicy(IPolicyBuilder<IOnEntryBuilder> pb) =>
				pb.MultipleElements(ScxmlNs, name: "assign", (dr, b) => b.AddAction(dr.ReadAssign()))
				  .MultipleElements(ScxmlNs, name: "foreach", (dr, b) => b.AddAction(dr.ReadForEach()))
				  .MultipleElements(ScxmlNs, name: "if", (dr, b) => b.AddAction(dr.ReadIf()))
				  .MultipleElements(ScxmlNs, name: "log", (dr, b) => b.AddAction(dr.ReadLog()))
				  .MultipleElements(ScxmlNs, name: "raise", (dr, b) => b.AddAction(dr.ReadRaise()))
				  .MultipleElements(ScxmlNs, name: "send", (dr, b) => b.AddAction(dr.ReadSend()))
				  .MultipleElements(ScxmlNs, name: "cancel", (dr, b) => b.AddAction(dr.ReadCancel()))
				  .MultipleElements(ScxmlNs, name: "script", (dr, b) => b.AddAction(dr.ReadScript()))
				  .UnknownElement((dr, b) => b.AddAction(dr.ReadCustomAction()));

		private IOnExit ReadOnExit() => Populate(_factory.CreateOnExitBuilder(CreateAncestor()), OnExitPolicy).Build();

		private static void OnExitBuildPolicy(IPolicyBuilder<IOnExitBuilder> pb) =>
				pb.MultipleElements(ScxmlNs, name: "assign", (dr, b) => b.AddAction(dr.ReadAssign()))
				  .MultipleElements(ScxmlNs, name: "foreach", (dr, b) => b.AddAction(dr.ReadForEach()))
				  .MultipleElements(ScxmlNs, name: "if", (dr, b) => b.AddAction(dr.ReadIf()))
				  .MultipleElements(ScxmlNs, name: "log", (dr, b) => b.AddAction(dr.ReadLog()))
				  .MultipleElements(ScxmlNs, name: "raise", (dr, b) => b.AddAction(dr.ReadRaise()))
				  .MultipleElements(ScxmlNs, name: "send", (dr, b) => b.AddAction(dr.ReadSend()))
				  .MultipleElements(ScxmlNs, name: "cancel", (dr, b) => b.AddAction(dr.ReadCancel()))
				  .MultipleElements(ScxmlNs, name: "script", (dr, b) => b.AddAction(dr.ReadScript()))
				  .UnknownElement((dr, b) => b.AddAction(dr.ReadCustomAction()));

		private IInvoke ReadInvoke() => Populate(_factory.CreateInvokeBuilder(CreateAncestor()), InvokePolicy).Build();

		private static void InvokeBuildPolicy(IPolicyBuilder<IInvokeBuilder> pb) =>
				pb.OptionalAttribute(name: "type", (dr, b) => b.SetType(AsUri(dr.Current)))
				  .OptionalAttribute(name: "typeexpr", (dr, b) => b.SetTypeExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsUri(dr.Current)))
				  .OptionalAttribute(name: "srcexpr", (dr, b) => b.SetSourceExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "id", (dr, b) => b.SetId(dr.Current))
				  .OptionalAttribute(name: "idlocation", (dr, b) => b.SetIdLocation(dr.AsLocationExpression(dr.Current)))
				  .OptionalAttribute(name: "namelist", (dr, b) => b.SetNameList(dr.AsLocationExpressionList(dr.Current)))
				  .OptionalAttribute(name: "autoforward", (dr, b) => b.SetAutoForward(XmlConvert.ToBoolean(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "param", (dr, b) => b.AddParam(dr.ReadParam()))
				  .OptionalElement(ScxmlNs, name: "finalize", (dr, b) => b.SetFinalize(dr.ReadFinalize()))
				  .OptionalElement(ScxmlNs, name: "content", (dr, b) => b.SetContent(dr.ReadContent()));

		private IFinalize ReadFinalize() => Populate(_factory.CreateFinalizeBuilder(CreateAncestor()), FinalizePolicy).Build();

		private static void FinalizeBuildPolicy(IPolicyBuilder<IFinalizeBuilder> pb) =>
				pb.MultipleElements(ScxmlNs, name: "assign", (dr, b) => b.AddAction(dr.ReadAssign()))
				  .MultipleElements(ScxmlNs, name: "foreach", (dr, b) => b.AddAction(dr.ReadForEach()))
				  .MultipleElements(ScxmlNs, name: "if", (dr, b) => b.AddAction(dr.ReadIf()))
				  .MultipleElements(ScxmlNs, name: "log", (dr, b) => b.AddAction(dr.ReadLog()))
				  .MultipleElements(ScxmlNs, name: "cancel", (dr, b) => b.AddAction(dr.ReadCancel()))
				  .MultipleElements(ScxmlNs, name: "script", (dr, b) => b.AddAction(dr.ReadScript()))
				  .UnknownElement((dr, b) => b.AddAction(dr.ReadCustomAction()));

		private IScript ReadScript() => Populate(_factory.CreateScriptBuilder(CreateAncestor()), ScriptPolicy).Build();

		private static void ScriptBuildPolicy(IPolicyBuilder<IScriptBuilder> pb) =>
				pb.OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsExternalScriptExpression(dr.Current)))
				  .RawContent((dr, b) => b.SetBody(dr.AsScriptExpression(dr.Current)));

		private IDataModel ReadDataModel() => Populate(_factory.CreateDataModelBuilder(CreateAncestor()), DataModelPolicy).Build();

		private static void DataModelBuildPolicy(IPolicyBuilder<IDataModelBuilder> pb) => pb.MultipleElements(ScxmlNs, name: "data", (dr, b) => b.AddData(dr.ReadData()));

		private IData ReadData() => Populate(_factory.CreateDataBuilder(CreateAncestor()), DataPolicy).Build();

		private static void DataBuildPolicy(IPolicyBuilder<IDataBuilder> pb) =>
				pb.RequiredAttribute(name: "id", delegate(ScxmlDirector dr, IDataBuilder b) { b.SetId(dr.Current); })
				  .OptionalAttribute(name: "src", (dr, b) => b.SetSource(AsExternalDataExpression(dr.Current)))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.Current)))
				  .RawContent((dr, b) => b.SetInlineContent(dr.AsInlineContent(dr.Current)));

		private IDoneData ReadDoneData() => Populate(_factory.CreateDoneDataBuilder(CreateAncestor()), DoneDataPolicy).Build();

		private static void DoneDataBuildPolicy(IPolicyBuilder<IDoneDataBuilder> pb) =>
				pb.OptionalElement(ScxmlNs, name: "content", (dr, b) => b.SetContent(dr.ReadContent()))
				  .MultipleElements(ScxmlNs, name: "param", (dr, b) => b.AddParameter(dr.ReadParam()));

		private IForEach ReadForEach() => Populate(_factory.CreateForEachBuilder(CreateAncestor()), ForEachPolicy).Build();

		private static void ForEachBuildPolicy(IPolicyBuilder<IForEachBuilder> pb) =>
				pb.RequiredAttribute(name: "array", (dr, b) => b.SetArray(dr.AsValueExpression(dr.Current)))
				  .RequiredAttribute(name: "item", (dr, b) => b.SetItem(dr.AsLocationExpression(dr.Current)))
				  .OptionalAttribute(name: "index", (dr, b) => b.SetIndex(dr.AsLocationExpression(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "assign", (dr, b) => b.AddAction(dr.ReadAssign()))
				  .MultipleElements(ScxmlNs, name: "foreach", (dr, b) => b.AddAction(dr.ReadForEach()))
				  .MultipleElements(ScxmlNs, name: "if", (dr, b) => b.AddAction(dr.ReadIf()))
				  .MultipleElements(ScxmlNs, name: "log", (dr, b) => b.AddAction(dr.ReadLog()))
				  .MultipleElements(ScxmlNs, name: "raise", (dr, b) => b.AddAction(dr.ReadRaise()))
				  .MultipleElements(ScxmlNs, name: "send", (dr, b) => b.AddAction(dr.ReadSend()))
				  .MultipleElements(ScxmlNs, name: "cancel", (dr, b) => b.AddAction(dr.ReadCancel()))
				  .MultipleElements(ScxmlNs, name: "script", (dr, b) => b.AddAction(dr.ReadScript()))
				  .UnknownElement((dr, b) => b.AddAction(dr.ReadCustomAction()));

		private IIf ReadIf() => Populate(_factory.CreateIfBuilder(CreateAncestor()), IfPolicy).Build();

		private static void IfBuildPolicy(IPolicyBuilder<IIfBuilder> pb) =>
				pb.RequiredAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.Current)))
				  .MultipleElements(ScxmlNs, name: "elseif", (dr, b) => b.AddAction(dr.ReadElseIf()))
				  .MultipleElements(ScxmlNs, name: "else", (dr, b) => b.AddAction(dr.ReadElse()))
				  .MultipleElements(ScxmlNs, name: "assign", (dr, b) => b.AddAction(dr.ReadAssign()))
				  .MultipleElements(ScxmlNs, name: "foreach", (dr, b) => b.AddAction(dr.ReadForEach()))
				  .MultipleElements(ScxmlNs, name: "if", (dr, b) => b.AddAction(dr.ReadIf()))
				  .MultipleElements(ScxmlNs, name: "log", (dr, b) => b.AddAction(dr.ReadLog()))
				  .MultipleElements(ScxmlNs, name: "raise", (dr, b) => b.AddAction(dr.ReadRaise()))
				  .MultipleElements(ScxmlNs, name: "send", (dr, b) => b.AddAction(dr.ReadSend()))
				  .MultipleElements(ScxmlNs, name: "cancel", (dr, b) => b.AddAction(dr.ReadCancel()))
				  .MultipleElements(ScxmlNs, name: "script", (dr, b) => b.AddAction(dr.ReadScript()))
				  .UnknownElement((dr, b) => b.AddAction(dr.ReadCustomAction()));

		private IElse ReadElse() => Populate(_factory.CreateElseBuilder(CreateAncestor()), ElsePolicy).Build();

		private static void ElseBuildPolicy(IPolicyBuilder<IElseBuilder> pb) { }

		private IElseIf ReadElseIf() => Populate(_factory.CreateElseIfBuilder(CreateAncestor()), ElseIfPolicy).Build();

		private static void ElseIfBuildPolicy(IPolicyBuilder<IElseIfBuilder> pb) => pb.RequiredAttribute(name: "cond", (dr, b) => b.SetCondition(dr.AsConditionalExpression(dr.Current)));

		private IRaise ReadRaise() => Populate(_factory.CreateRaiseBuilder(CreateAncestor()), RaisePolicy).Build();

		private static void RaiseBuildPolicy(IPolicyBuilder<IRaiseBuilder> pb) => pb.RequiredAttribute(name: "event", (dr, b) => b.SetEvent(AsEvent(dr.Current)));

		private IAssign ReadAssign() => Populate(_factory.CreateAssignBuilder(CreateAncestor()), AssignPolicy).Build();

		private static void AssignBuildPolicy(IPolicyBuilder<IAssignBuilder> pb) =>
				pb.RequiredAttribute(name: "location", (dr, b) => b.SetLocation(dr.AsLocationExpression(dr.Current)))
				  .OptionalAttribute(name: "expr", (dr, b) => b.SetExpression(dr.AsValueExpression(dr.Current)))
				  .OptionalAttribute(name: "type", (dr, b) => b.SetType(dr.Current))
				  .OptionalAttribute(name: "attr", (dr, b) => b.SetAttribute(dr.Current))
				  .RawContent((dr, b) => b.SetInlineContent(dr.AsInlineContent(dr.Current)));

		private ICancel ReadCancel() => Populate(_factory.CreateCancelBuilder(CreateAncestor()), CancelPolicy).Build();

		private static void CancelBuildPolicy(IPolicyBuilder<ICancelBuilder> pb) =>
				pb.OptionalAttribute(name: "sendid", (dr, b) => b.SetSendId(dr.Current))
				  .OptionalAttribute(name: "sendidexpr", (dr, b) => b.SetSendIdExpression(dr.AsValueExpression(dr.Current)));

		private ICustomAction ReadCustomAction()
		{
			var builder = _factory.CreateCustomActionBuilder(CreateAncestor(namespaces: true, nameTable: true));
			builder.SetXml(ReadOuterXml());
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