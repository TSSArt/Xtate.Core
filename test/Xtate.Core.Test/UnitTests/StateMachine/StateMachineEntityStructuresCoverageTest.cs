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

using System.Collections.Immutable;
using Xtate.Ancestor;
using Xtate.StateMachine;
using Xtate.StateMachine.Internal;

namespace Xtate.Test.UnitTests.StateMachine;

[TestClass]
public class StateMachineEntityStructuresCoverageTest
{
	[TestMethod]
	public void DataEntityCopiesSourceTracksAncestorDebugIdAndReferenceEquality()
	{
		var source = new DataSource
					 {
						 Id = "data1",
						 Source = new ExternalDataExpressionSource { Uri = new Uri("https://example.test/data") },
						 Expression = new ValueExpressionSource { Expression = "1 + 1" },
						 InlineContent = new InlineContentSource { Value = "inline" }
					 };

		var entity = Init<DataEntity, IData>(source);
		var same = Init<DataEntity, IData>(source);
		var different = same;
		different.Id = "data2";

		Assert.AreSame(source, ((IAncestorProvider) entity).Ancestor);
		Assert.AreSame(source.Id, entity.Id);
		Assert.AreSame(source.Source, entity.Source);
		Assert.AreSame(source.Expression, entity.Expression);
		Assert.AreSame(source.InlineContent, entity.InlineContent);
		Assert.AreEqual("data1", ((IDebugEntityId) entity).EntityId.ToString());
		Assert.IsTrue(RefEquals<DataEntity, IData>(entity, same));
		Assert.IsFalse(RefEquals<DataEntity, IData>(entity, different));
	}

	[TestMethod]
	public void HistoryEntityCopiesSourceTracksAncestorDebugIdAndReferenceEquality()
	{
		var source = new HistorySource
					 {
						 Id = Identifier.FromString("hist"),
						 Type = HistoryType.Deep,
						 Transition = new TransitionSource { Type = TransitionType.Internal }
					 };

		var entity = Init<HistoryEntity, IHistory>(source);
		var same = Init<HistoryEntity, IHistory>(source);
		var different = same;
		different.Type = HistoryType.Shallow;

		Assert.AreSame(source, ((IAncestorProvider) entity).Ancestor);
		Assert.AreSame(source.Id, entity.Id);
		Assert.AreEqual(HistoryType.Deep, entity.Type);
		Assert.AreSame(source.Transition, entity.Transition);
		Assert.AreEqual("hist", ((IDebugEntityId) entity).EntityId.ToString());
		Assert.IsTrue(RefEquals<HistoryEntity, IHistory>(entity, same));
		Assert.IsFalse(RefEquals<HistoryEntity, IHistory>(entity, different));
	}

	[TestMethod]
	public void FinalEntityCopiesSourceTracksAncestorDebugIdAndReferenceEquality()
	{
		var onEntry = new OnEntrySource();
		var onExit = new OnExitSource();
		var doneData = new DoneDataSource();
		var source = new FinalSource
					 {
						 Id = Identifier.FromString("final"),
						 OnEntry = [onEntry],
						 OnExit = [onExit],
						 DoneData = doneData
					 };

		var entity = Init<FinalEntity, IFinal>(source);
		var same = Init<FinalEntity, IFinal>(source);
		var different = same;
		different.DoneData = new DoneDataSource();

		Assert.AreSame(source, ((IAncestorProvider) entity).Ancestor);
		Assert.AreSame(source.Id, entity.Id);
		Assert.AreSame(onEntry, entity.OnEntry[0]);
		Assert.AreSame(onExit, entity.OnExit[0]);
		Assert.AreSame(doneData, entity.DoneData);
		Assert.AreEqual("final", ((IDebugEntityId) entity).EntityId.ToString());
		Assert.IsTrue(RefEquals<FinalEntity, IFinal>(entity, same));
		Assert.IsFalse(RefEquals<FinalEntity, IFinal>(entity, different));
	}

	[TestMethod]
	public void TransitionEntityCopiesSourceTracksAncestorAndReferenceEquality()
	{
		var condition = new ConditionExpressionSource { Expression = "cond" };
		var action = new ValueExpressionSource { Expression = "action" };
		var eventDescriptor = EventDescriptor.FromString("event.name");
		var target = Identifier.FromString("target");
		var source = new TransitionSource
					 {
						 EventDescriptors = [eventDescriptor],
						 Condition = condition,
						 Target = [target],
						 Type = TransitionType.External,
						 Action = [action]
					 };

		var entity = Init<TransitionEntity, ITransition>(source);
		var same = Init<TransitionEntity, ITransition>(source);
		var different = same;
		different.Type = TransitionType.Internal;

		Assert.AreSame(source, ((IAncestorProvider) entity).Ancestor);
		Assert.AreSame(eventDescriptor, entity.EventDescriptors[0]);
		Assert.AreSame(condition, entity.Condition);
		Assert.AreSame(target, entity.Target[0]);
		Assert.AreEqual(TransitionType.External, entity.Type);
		Assert.AreSame(action, entity.Action[0]);
		Assert.IsTrue(RefEquals<TransitionEntity, ITransition>(entity, same));
		Assert.IsFalse(RefEquals<TransitionEntity, ITransition>(entity, different));
	}

	[TestMethod]
	public void ContentDoneDataAndDataModelEntitiesCopySourceAndCompareReferences()
	{
		var valueExpression = new ValueExpressionSource { Expression = "content expr" };
		var contentBody = new ContentBodySource { Value = "content body" };
		var contentSource = new ContentSource { Expression = valueExpression, Body = contentBody };
		var content = Init<ContentEntity, IContent>(contentSource);
		var sameContent = Init<ContentEntity, IContent>(contentSource);
		var differentContent = sameContent;
		differentContent.Body = new ContentBodySource();

		Assert.AreSame(contentSource, ((IAncestorProvider) content).Ancestor);
		Assert.AreSame(valueExpression, content.Expression);
		Assert.AreSame(contentBody, content.Body);
		Assert.IsTrue(RefEquals<ContentEntity, IContent>(content, sameContent));
		Assert.IsFalse(RefEquals<ContentEntity, IContent>(content, differentContent));

		var param = new ParamSource { Name = "p" };
		var doneDataSource = new DoneDataSource { Content = contentSource, Parameters = [param] };
		var doneData = Init<DoneDataEntity, IDoneData>(doneDataSource);
		var sameDoneData = Init<DoneDataEntity, IDoneData>(doneDataSource);
		var differentDoneData = sameDoneData;
		differentDoneData.Content = new ContentSource();

		Assert.AreSame(doneDataSource, ((IAncestorProvider) doneData).Ancestor);
		Assert.AreSame(contentSource, doneData.Content);
		Assert.AreSame(param, doneData.Parameters[0]);
		Assert.IsTrue(RefEquals<DoneDataEntity, IDoneData>(doneData, sameDoneData));
		Assert.IsFalse(RefEquals<DoneDataEntity, IDoneData>(doneData, differentDoneData));

		var dataSource = new DataSource { Id = "data" };
		var dataModelSource = new DataModelSource { Data = [dataSource] };
		var dataModel = Init<DataModelEntity, IDataModel>(dataModelSource);
		var sameDataModel = Init<DataModelEntity, IDataModel>(dataModelSource);
		var differentDataModel = sameDataModel;
		differentDataModel.Data = [];

		Assert.AreSame(dataModelSource, ((IAncestorProvider) dataModel).Ancestor);
		Assert.AreSame(dataSource, dataModel.Data[0]);
		Assert.IsTrue(RefEquals<DataModelEntity, IDataModel>(dataModel, sameDataModel));
		Assert.IsFalse(RefEquals<DataModelEntity, IDataModel>(dataModel, differentDataModel));
	}

	[TestMethod]
	public void SimpleExecutableEntitiesCopySourceAndCompareReferences()
	{
		var condition = new ConditionExpressionSource { Expression = "condition" };
		var elseIfSource = new ElseIfSource { Condition = condition };
		var elseIf = Init<ElseIfEntity, IElseIf>(elseIfSource);
		var sameElseIf = Init<ElseIfEntity, IElseIf>(elseIfSource);
		var differentElseIf = sameElseIf;
		differentElseIf.Condition = new ConditionExpressionSource();

		Assert.AreSame(elseIfSource, ((IAncestorProvider) elseIf).Ancestor);
		Assert.AreSame(condition, elseIf.Condition);
		Assert.IsTrue(RefEquals<ElseIfEntity, IElseIf>(elseIf, sameElseIf));
		Assert.IsFalse(RefEquals<ElseIfEntity, IElseIf>(elseIf, differentElseIf));

		var transition = new TransitionSource { Type = TransitionType.Internal };
		var initialSource = new InitialSource { Transition = transition };
		var initial = Init<InitialEntity, IInitial>(initialSource);
		var sameInitial = Init<InitialEntity, IInitial>(initialSource);
		var differentInitial = sameInitial;
		differentInitial.Transition = new TransitionSource();

		Assert.AreSame(initialSource, ((IAncestorProvider) initial).Ancestor);
		Assert.AreSame(transition, initial.Transition);
		Assert.IsTrue(RefEquals<InitialEntity, IInitial>(initial, sameInitial));
		Assert.IsFalse(RefEquals<InitialEntity, IInitial>(initial, differentInitial));

		var action = new ValueExpressionSource { Expression = "action" };
		var onEntrySource = new OnEntrySource { Action = [action] };
		var onEntry = Init<OnEntryEntity, IOnEntry>(onEntrySource);
		var sameOnEntry = Init<OnEntryEntity, IOnEntry>(onEntrySource);
		var differentOnEntry = sameOnEntry;
		differentOnEntry.Action = [];

		Assert.AreSame(onEntrySource, ((IAncestorProvider) onEntry).Ancestor);
		Assert.AreSame(action, onEntry.Action[0]);
		Assert.IsTrue(RefEquals<OnEntryEntity, IOnEntry>(onEntry, sameOnEntry));
		Assert.IsFalse(RefEquals<OnEntryEntity, IOnEntry>(onEntry, differentOnEntry));

		var onExitSource = new OnExitSource { Action = [action] };
		var onExit = Init<OnExitEntity, IOnExit>(onExitSource);
		var sameOnExit = Init<OnExitEntity, IOnExit>(onExitSource);
		var differentOnExit = sameOnExit;
		differentOnExit.Action = [];

		Assert.AreSame(onExitSource, ((IAncestorProvider) onExit).Ancestor);
		Assert.AreSame(action, onExit.Action[0]);
		Assert.IsTrue(RefEquals<OnExitEntity, IOnExit>(onExit, sameOnExit));
		Assert.IsFalse(RefEquals<OnExitEntity, IOnExit>(onExit, differentOnExit));
	}

	[TestMethod]
	public void ParamEntityCopiesSourceAndComparesNameExpressionAndLocationReferences()
	{
		var expression = new ValueExpressionSource { Expression = "expr" };
		var location = new LocationExpressionSource { Expression = "loc" };
		var source = new ParamSource
					 {
						 Name = "param",
						 Expression = expression,
						 Location = location
					 };

		var entity = Init<ParamEntity, IParam>(source);
		var same = Init<ParamEntity, IParam>(source);
		var different = same;
		different.Name = string.Copy("param");

		Assert.AreSame(source, ((IAncestorProvider) entity).Ancestor);
		Assert.AreSame(source.Name, entity.Name);
		Assert.AreSame(expression, entity.Expression);
		Assert.AreSame(location, entity.Location);
		Assert.IsTrue(RefEquals<ParamEntity, IParam>(entity, same));
		Assert.IsFalse(RefEquals<ParamEntity, IParam>(entity, different));
	}

	[TestMethod]
	public void ExpressionEntitiesCopySourceAndCompareReferences()
	{
		var dataUri = new Uri("https://example.test/data.json");
		var externalDataSource = new ExternalDataExpressionSource { Uri = dataUri };
		var externalData = Init<ExternalDataExpression, IExternalDataExpression>(externalDataSource);
		var sameExternalData = Init<ExternalDataExpression, IExternalDataExpression>(externalDataSource);
		var differentExternalData = sameExternalData;
		differentExternalData.Uri = new Uri(dataUri.ToString());

		Assert.AreSame(externalDataSource, ((IAncestorProvider) externalData).Ancestor);
		Assert.AreSame(dataUri, externalData.Uri);
		Assert.IsTrue(RefEquals<ExternalDataExpression, IExternalDataExpression>(externalData, sameExternalData));
		Assert.IsFalse(RefEquals<ExternalDataExpression, IExternalDataExpression>(externalData, differentExternalData));

		var scriptUri = new Uri("https://example.test/script.js");
		var externalScriptSource = new ExternalScriptExpressionSource { Uri = scriptUri };
		var externalScript = Init<ExternalScriptExpression, IExternalScriptExpression>(externalScriptSource);
		var sameExternalScript = Init<ExternalScriptExpression, IExternalScriptExpression>(externalScriptSource);
		var differentExternalScript = sameExternalScript;
		differentExternalScript.Uri = new Uri(scriptUri.ToString());

		Assert.AreSame(externalScriptSource, ((IAncestorProvider) externalScript).Ancestor);
		Assert.AreSame(scriptUri, externalScript.Uri);
		Assert.IsTrue(RefEquals<ExternalScriptExpression, IExternalScriptExpression>(externalScript, sameExternalScript));
		Assert.IsFalse(RefEquals<ExternalScriptExpression, IExternalScriptExpression>(externalScript, differentExternalScript));

		var scriptSource = new ScriptExpressionSource { Expression = "script()" };
		var script = Init<ScriptExpression, IScriptExpression>(scriptSource);
		var sameScript = Init<ScriptExpression, IScriptExpression>(scriptSource);
		var differentScript = sameScript;
		differentScript.Expression = string.Copy("script()");

		Assert.AreSame(scriptSource, ((IAncestorProvider) script).Ancestor);
		Assert.AreSame(scriptSource.Expression, script.Expression);
		Assert.IsTrue(RefEquals<ScriptExpression, IScriptExpression>(script, sameScript));
		Assert.IsFalse(RefEquals<ScriptExpression, IScriptExpression>(script, differentScript));
	}

	[TestMethod]
	public void ElseEntityCopiesAncestorAndAlwaysReferenceEquals()
	{
		var source = new ElseSource();
		var otherSource = new ElseSource();

		var entity = Init<ElseEntity, IElse>(source);
		var same = Init<ElseEntity, IElse>(source);
		var differentSourceEntity = Init<ElseEntity, IElse>(otherSource);

		Assert.AreSame(source, ((IAncestorProvider) entity).Ancestor);
		Assert.IsTrue(RefEquals<ElseEntity, IElse>(entity, same));
		Assert.IsTrue(RefEquals<ElseEntity, IElse>(entity, differentSourceEntity));
	}

	[TestMethod]
	public void SendAndCancelEntitiesCopySourceAndCompareReferences()
	{
		var idLocation = new LocationExpressionSource { Expression = "sendIdLocation" };
		var eventExpression = new ValueExpressionSource { Expression = "eventExpression" };
		var content = new ContentSource();
		var target = new FullUri("https://example.test/target");
		var targetExpression = new ValueExpressionSource { Expression = "targetExpression" };
		var type = new FullUri("https://example.test/type");
		var typeExpression = new ValueExpressionSource { Expression = "typeExpression" };
		var param = new ParamSource { Name = "param" };
		var delayExpression = new ValueExpressionSource { Expression = "delayExpression" };
		var nameListItem = new LocationExpressionSource { Expression = "name" };
		var sendSource = new SendSource
						 {
							 Id = "send",
							 IdLocation = idLocation,
							 EventName = "event",
							 EventExpression = eventExpression,
							 Content = content,
							 Target = target,
							 TargetExpression = targetExpression,
							 Type = type,
							 TypeExpression = typeExpression,
							 Parameters = [param],
							 DelayMs = 123,
							 DelayExpression = delayExpression,
							 NameList = [nameListItem]
						 };

		var send = Init<SendEntity, ISend>(sendSource);
		var sameSend = Init<SendEntity, ISend>(sendSource);
		var differentSend = sameSend;
		differentSend.Id = string.Copy("send");

		Assert.AreSame(sendSource, ((IAncestorProvider) send).Ancestor);
		Assert.AreSame(sendSource.Id, send.Id);
		Assert.AreSame(idLocation, send.IdLocation);
		Assert.AreSame(sendSource.EventName, send.EventName);
		Assert.AreSame(eventExpression, send.EventExpression);
		Assert.AreSame(content, send.Content);
		Assert.AreSame(target, send.Target);
		Assert.AreSame(targetExpression, send.TargetExpression);
		Assert.AreSame(type, send.Type);
		Assert.AreSame(typeExpression, send.TypeExpression);
		Assert.AreSame(param, send.Parameters[0]);
		Assert.AreEqual(123, send.DelayMs);
		Assert.AreSame(delayExpression, send.DelayExpression);
		Assert.AreSame(nameListItem, send.NameList[0]);
		Assert.AreEqual("send", ((IDebugEntityId) send).EntityId.ToString());
		Assert.IsTrue(RefEquals<SendEntity, ISend>(send, sameSend));
		Assert.IsFalse(RefEquals<SendEntity, ISend>(send, differentSend));

		var sendIdExpression = new ValueExpressionSource { Expression = "sendIdExpression" };
		var cancelSource = new CancelSource { SendId = "send", SendIdExpression = sendIdExpression };
		var cancel = Init<CancelEntity, ICancel>(cancelSource);
		var sameCancel = Init<CancelEntity, ICancel>(cancelSource);
		var differentCancel = sameCancel;
		differentCancel.SendId = string.Copy("send");

		Assert.AreSame(cancelSource, ((IAncestorProvider) cancel).Ancestor);
		Assert.AreSame(cancelSource.SendId, cancel.SendId);
		Assert.AreSame(sendIdExpression, cancel.SendIdExpression);
		Assert.IsTrue(RefEquals<CancelEntity, ICancel>(cancel, sameCancel));
		Assert.IsFalse(RefEquals<CancelEntity, ICancel>(cancel, differentCancel));
	}

	private static TEntity Init<TEntity, TInterface>(TInterface source)
		where TEntity : struct, IVisitorEntity<TEntity, TInterface>, TInterface
	{
		var entity = default(TEntity);
		entity.Init(source);

		return entity;
	}

	private static bool RefEquals<TEntity, TInterface>(TEntity left, TEntity right)
		where TEntity : struct, IVisitorEntity<TEntity, TInterface>, TInterface =>
		left.RefEquals(ref right);

	private sealed class DataSource : IData
	{
		public string? Id { get; init; }

		public IExternalDataExpression? Source { get; init; }

		public IValueExpression? Expression { get; init; }

		public IInlineContent? InlineContent { get; init; }
	}

	private sealed class HistorySource : IHistory
	{
		public IIdentifier? Id { get; init; }

		public HistoryType Type { get; init; }

		public ITransition? Transition { get; init; }
	}

	private sealed class FinalSource : IFinal
	{
		public IIdentifier? Id { get; init; }

		public ImmutableArray<IOnEntry> OnEntry { get; init; }

		public ImmutableArray<IOnExit> OnExit { get; init; }

		public IDoneData? DoneData { get; init; }
	}

	private sealed class TransitionSource : ITransition
	{
		public EventDescriptors EventDescriptors { get; init; }

		public IConditionExpression? Condition { get; init; }

		public Target Target { get; init; }

		public TransitionType Type { get; init; }

		public ImmutableArray<IExecutableEntity> Action { get; init; }
	}

	private sealed class ConditionExpressionSource : IConditionExpression
	{
		public string? Expression { get; init; }
	}

	private sealed class ValueExpressionSource : IValueExpression
	{
		public string? Expression { get; init; }
	}

	private sealed class ExternalDataExpressionSource : IExternalDataExpression
	{
		public Uri? Uri { get; init; }
	}

	private sealed class ExternalScriptExpressionSource : IExternalScriptExpression
	{
		public Uri? Uri { get; init; }
	}

	private sealed class ScriptExpressionSource : IScriptExpression
	{
		public string? Expression { get; init; }
	}

	private sealed class InlineContentSource : IInlineContent
	{
		public string? Value { get; init; }
	}

	private sealed class DoneDataSource : IDoneData
	{
		public IContent? Content { get; init; }

		public ImmutableArray<IParam> Parameters { get; init; }
	}

	private sealed class OnEntrySource : IOnEntry
	{
		public ImmutableArray<IExecutableEntity> Action { get; init; }
	}

	private sealed class OnExitSource : IOnExit
	{
		public ImmutableArray<IExecutableEntity> Action { get; init; }
	}

	private sealed class ContentSource : IContent
	{
		public IValueExpression? Expression { get; init; }

		public IContentBody? Body { get; init; }
	}

	private sealed class ContentBodySource : IContentBody
	{
		public string? Value { get; init; }
	}

	private sealed class DataModelSource : IDataModel
	{
		public ImmutableArray<IData> Data { get; init; }
	}

	private sealed class ElseIfSource : IElseIf
	{
		public IConditionExpression? Condition { get; init; }
	}

	private sealed class ElseSource : IElse;

	private sealed class InitialSource : IInitial
	{
		public ITransition? Transition { get; init; }
	}

	private sealed class ParamSource : IParam
	{
		public string Name { get; init; } = null!;

		public IValueExpression? Expression { get; init; }

		public ILocationExpression? Location { get; init; }
	}

	private sealed class LocationExpressionSource : ILocationExpression
	{
		public string? Expression { get; init; }
	}

	private sealed class SendSource : ISend
	{
		public IContent? Content { get; init; }

		public IValueExpression? DelayExpression { get; init; }

		public int? DelayMs { get; init; }

		public string? EventName { get; init; }

		public IValueExpression? EventExpression { get; init; }

		public string? Id { get; init; }

		public ILocationExpression? IdLocation { get; init; }

		public ImmutableArray<ILocationExpression> NameList { get; init; }

		public ImmutableArray<IParam> Parameters { get; init; }

		public FullUri? Target { get; init; }

		public IValueExpression? TargetExpression { get; init; }

		public FullUri? Type { get; init; }

		public IValueExpression? TypeExpression { get; init; }
	}

	private sealed class CancelSource : ICancel
	{
		public string? SendId { get; init; }

		public IValueExpression? SendIdExpression { get; init; }
	}
}
