// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.DataModel;

namespace Xtate.Persistence;

public interface IEntityMap
{
	bool TryGetEntityByDocumentId(int id, [MaybeNullWhen(false)] out IEntity entity);
}

public class StateMachineReader
{
	private IEntityMap? _forwardEntities;

	public IStateMachine Build(Bucket bucket, IEntityMap? forwardEntities = default)
	{
		_forwardEntities = forwardEntities;

		return RestoreStateMachine(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement);
	}

	private static bool Exist(in Bucket bucket, TypeInfo typeInfo)
	{
		if (bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo))
		{
			if (storedTypeInfo != typeInfo)
			{
				throw new PersistenceException(Resources.Exception_UnexpectedTypeInfoValue);
			}

			return true;
		}

		return false;
	}

	private IExecutableEntity ForwardExecEntity(Bucket bucket)
	{
		var documentId = bucket.GetInt32(Key.DocumentId);

		if (_forwardEntities is null)
		{
			throw new PersistenceException(Resources.Exception_ForwardEntitiesRequiredToRestoreStateMachine);
		}

		if (!_forwardEntities.TryGetEntityByDocumentId(documentId, out var entity))
		{
			throw new PersistenceException(Resources.Exception_ForwardEntityCanNotBeFound);
		}

		if (entity is not IExecutableEntity executableEntity)
		{
			throw new PersistenceException(Resources.Exception_ForwardEntityHasIncorrectType);
		}

		return executableEntity;
	}

	private IStateMachine? RestoreStateMachine(Bucket bucket) =>
		Exist(bucket, TypeInfo.StateMachineNode)
			? new StateMachineEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Name = bucket.GetString(Key.Name),
				  DataModelType = bucket.GetString(Key.DataModelType),
				  Binding = bucket.GetEnum(Key.Binding).As<BindingType>(),
				  Script = RestoreScript(bucket.Nested(Key.Script)),
				  DataModel = RestoreDataModel(bucket.Nested(Key.DataModel)),
				  Initial = RestoreInitial(bucket.Nested(Key.Initial)),
				  States = bucket.RestoreList(Key.States, RestoreStateEntity)
			  }
			: null;

	private static IDataModel? RestoreDataModel(Bucket bucket) =>
		Exist(bucket, TypeInfo.DataModelNode)
			? new DataModelEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Data = bucket.RestoreList(Key.DataList, RestoreData)
			  }
			: null;

	private IInitial? RestoreInitial(Bucket bucket) =>
		Exist(bucket, TypeInfo.InitialNode)
			? new InitialEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Transition = RestoreTransition(bucket.Nested(Key.Transition))
			  }
			: null;

	private ITransition? RestoreTransition(Bucket bucket) =>
		Exist(bucket, TypeInfo.TransitionNode)
			? new TransitionEntity
			  {
				  Ancestor = new EntityData(bucket),
				  EventDescriptors = bucket.RestoreList(Key.Event, RestoreEventDescriptor),
				  Condition = RestoreCondition(bucket.Nested(Key.Condition)),
				  Target = bucket.RestoreList(Key.Target, RestoreIdentifier),
				  Type = bucket.GetEnum(Key.TransitionType).As<TransitionType>(),
				  Action = bucket.RestoreList(Key.Action, RestoreExecutableEntity)
			  }
			: null;

	private static IAssign? RestoreAssign(Bucket bucket) =>
		Exist(bucket, TypeInfo.AssignNode)
			? new AssignEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Location = RestoreLocationExpression(bucket.Nested(Key.Location)),
				  Expression = RestoreValueExpression(bucket.Nested(Key.Expression)),
				  Type = bucket.GetString(Key.Type),
				  Attribute = bucket.GetString(Key.Attribute),
				  InlineContent = bucket.TryGet(Key.InlineContent, out string? content) ? new InlineContent { Value = content } : null
			  }
			: null;

	private static ICancel? RestoreCancel(Bucket bucket) =>
		Exist(bucket, TypeInfo.CancelNode)
			? new CancelEntity
			  {
				  Ancestor = new EntityData(bucket),
				  SendId = bucket.GetString(Key.SendId),
				  SendIdExpression = RestoreValueExpression(bucket.Nested(Key.SendIdExpression))
			  }
			: null;

	private IState? RestoreCompound(Bucket bucket) =>
		Exist(bucket, TypeInfo.CompoundNode)
			? new StateEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = RestoreIdentifier(bucket.Nested(Key.Id)),
				  Initial = RestoreInitial(bucket.Nested(Key.Initial)),
				  DataModel = RestoreDataModel(bucket.Nested(Key.DataModel)),
				  States = bucket.RestoreList(Key.States, RestoreStateEntity),
				  HistoryStates = bucket.RestoreList(Key.HistoryStates, RestoreHistory),
				  Transitions = bucket.RestoreList(Key.Transitions, RestoreTransition),
				  OnEntry = bucket.RestoreList(Key.OnEntry, RestoreOnEntry),
				  OnExit = bucket.RestoreList(Key.OnExit, RestoreOnExit),
				  Invoke = bucket.RestoreList(Key.Invoke, RestoreInvoke)
			  }
			: null;

	private IConditionExpression? RestoreCondition(Bucket bucket)
	{
		if (!bucket.TryGet(Key.TypeInfo, out TypeInfo typeInfo))
		{
			return null;
		}

		return typeInfo switch
			   {
				   TypeInfo.ConditionExpressionNode => RestoreConditionExpression(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.RuntimeExecNode         => (IConditionExpression) ForwardExecEntity(bucket),
				   _                                => throw new PersistenceException(Resources.Exception_UnknownConditionType)
			   };
	}

	private static IConditionExpression? RestoreConditionExpression(Bucket bucket) =>
		Exist(bucket, TypeInfo.ConditionExpressionNode)
			? new ConditionExpression
			  {
				  Ancestor = new EntityData(bucket),
				  Expression = bucket.GetString(Key.Expression)
			  }
			: null;

	private static IContent? RestoreContent(Bucket bucket) =>
		Exist(bucket, TypeInfo.ContentNode)
			? new ContentEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Expression = RestoreValueExpression(bucket.Nested(Key.Expression)),
				  Body = bucket.TryGet(Key.Body, out string? body) ? new ContentBody { Value = body } : null
			  }
			: null;

	private static IData? RestoreData(Bucket bucket) =>
		Exist(bucket, TypeInfo.DataNode)
			? new DataEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = bucket.GetString(Key.Id),
				  Source = RestoreExternalDataExpression(bucket.Nested(Key.Source)),
				  Expression = RestoreValueExpression(bucket.Nested(Key.Expression)),
				  InlineContent = bucket.TryGet(Key.InlineContent, out string? content) ? new InlineContent { Value = content } : null
			  }
			: null;

	private static IDoneData? RestoreDoneData(Bucket bucket) =>
		Exist(bucket, TypeInfo.DoneDataNode)
			? new DoneDataEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Content = RestoreContent(bucket.Nested(Key.Source)),
				  Parameters = bucket.RestoreList(Key.Parameters, RestoreParam)
			  }
			: null;

	private static IElseIf? RestoreElseIf(Bucket bucket) =>
		Exist(bucket, TypeInfo.ElseIfNode)
			? new ElseIfEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Condition = RestoreConditionExpression(bucket.Nested(Key.Condition))
			  }
			: null;

	private static IElse? RestoreElse(Bucket bucket) =>
		Exist(bucket, TypeInfo.ElseNode)
			? new ElseEntity
			  {
				  Ancestor = new EntityData(bucket)
			  }
			: null;

	private static IEventDescriptor? RestoreEventDescriptor(Bucket bucket)
	{
		var value = bucket.GetString(Key.Id);

		return value is not null ? (EventDescriptor) value : null;
	}

	private static IOutgoingEvent RestoreEvent(Bucket bucket) => new EventEntity(bucket.GetString(Key.Id)) { Target = EventEntity.InternalTarget };

	private IExecutableEntity RestoreExecutableEntity(Bucket bucket)
	{
		var typeInfo = bucket.GetEnum(Key.TypeInfo).As<TypeInfo>();
		return typeInfo switch
			   {
				   TypeInfo.AssignNode       => RestoreAssign(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.CancelNode       => RestoreCancel(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.CustomActionNode => RestoreCustomAction(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.ForEachNode      => RestoreForEach(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.IfNode           => RestoreIf(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.ElseIfNode       => RestoreElseIf(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.ElseNode         => RestoreElse(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.LogNode          => RestoreLog(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.RaiseNode        => RestoreRaise(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.ScriptNode       => RestoreScript(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.SendNode         => RestoreSend(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.RuntimeExecNode  => ForwardExecEntity(bucket),
				   _                         => throw new PersistenceException(Resources.Exception_UnknownExecutableEntityType)
			   };
	}

	private static ILog? RestoreLog(Bucket bucket) =>
		Exist(bucket, TypeInfo.LogNode)
			? new LogEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Label = bucket.GetString(Key.Label),
				  Expression = RestoreValueExpression(bucket.Nested(Key.Expression))
			  }
			: null;

	private static IRaise? RestoreRaise(Bucket bucket) =>
		Exist(bucket, TypeInfo.RaiseNode)
			? new RaiseEntity
			  {
				  Ancestor = new EntityData(bucket),
				  OutgoingEvent = RestoreEvent(bucket.Nested(Key.Event))
			  }
			: null;

	private static IScript? RestoreScript(Bucket bucket) =>
		Exist(bucket, TypeInfo.ScriptNode)
			? new ScriptEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Source = RestoreExternalScriptExpression(bucket.Nested(Key.Source)),
				  Content = RestoreScriptExpression(bucket.Nested(Key.Content))
			  }
			: null;

	private static ICustomAction? RestoreCustomAction(Bucket bucket) =>
		Exist(bucket, TypeInfo.CustomActionNode)
			? new CustomActionEntity
			  {
				  Ancestor = new EntityData(bucket),
				  XmlNamespace = bucket.GetString(Key.Namespace),
				  XmlName = bucket.GetString(Key.Name),
				  Xml = bucket.GetString(Key.Content),
				  Locations = bucket.RestoreList(Key.LocationList, RestoreLocationExpression),
				  Values = bucket.RestoreList(Key.ValueList, RestoreValueExpression)
			  }
			: null;

	private static ISend? RestoreSend(Bucket bucket) =>
		Exist(bucket, TypeInfo.SendNode)
			? new SendEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = bucket.GetString(Key.Id),
				  Type = bucket.GetUri(Key.Type),
				  EventName = bucket.GetString(Key.Event),
				  Target = bucket.GetUri(Key.Target),
				  DelayMs = bucket.GetInt32(Key.DelayMs),
				  TypeExpression = RestoreValueExpression(bucket.Nested(Key.TypeExpression)),
				  EventExpression = RestoreValueExpression(bucket.Nested(Key.EventExpression)),
				  TargetExpression = RestoreValueExpression(bucket.Nested(Key.TargetExpression)),
				  DelayExpression = RestoreValueExpression(bucket.Nested(Key.DelayExpression)),
				  IdLocation = RestoreLocationExpression(bucket.Nested(Key.IdLocation)),
				  NameList = bucket.RestoreList(Key.NameList, RestoreLocationExpression),
				  Parameters = bucket.RestoreList(Key.Parameters, RestoreParam),
				  Content = RestoreContent(bucket.Nested(Key.Content))
			  }
			: null;

	private static IExternalDataExpression? RestoreExternalDataExpression(Bucket bucket) =>
		Exist(bucket, TypeInfo.ExternalDataExpressionNode)
			? new ExternalDataExpression
			  {
				  Ancestor = new EntityData(bucket),
				  Uri = bucket.GetUri(Key.Uri)
			  }
			: null;

	private static IExternalScriptExpression? RestoreExternalScriptExpression(Bucket bucket)
	{
		if (!Exist(bucket, TypeInfo.ExternalScriptExpressionNode))
		{
			return null;
		}

		if (bucket.GetString(Key.Content) is { } content)
		{
			return new ExternalScriptExpressionWithContent(new EntityData(bucket), bucket.GetUri(Key.Uri), content);
		}

		return new ExternalScriptExpression
			   {
				   Ancestor = new EntityData(bucket),
				   Uri = bucket.GetUri(Key.Uri)
			   };
	}

	private IFinalize? RestoreFinalize(Bucket bucket) =>
		Exist(bucket, TypeInfo.FinalizeNode)
			? new FinalizeEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Action = bucket.RestoreList(Key.Parameters, RestoreExecutableEntity)
			  }
			: null;

	private IFinal? RestoreFinal(Bucket bucket) =>
		Exist(bucket, TypeInfo.FinalNode)
			? new FinalEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = RestoreIdentifier(bucket.Nested(Key.Id)),
				  OnEntry = bucket.RestoreList(Key.OnEntry, RestoreOnEntry),
				  OnExit = bucket.RestoreList(Key.OnExit, RestoreOnExit),
				  DoneData = RestoreDoneData(bucket.Nested(Key.DoneData))
			  }
			: null;

	private IForEach? RestoreForEach(Bucket bucket) =>
		Exist(bucket, TypeInfo.ForEachNode)
			? new ForEachEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Array = RestoreValueExpression(bucket.Nested(Key.Array)),
				  Item = RestoreLocationExpression(bucket.Nested(Key.Item)),
				  Index = RestoreLocationExpression(bucket.Nested(Key.Index)),
				  Action = bucket.RestoreList(Key.Action, RestoreExecutableEntity)
			  }
			: null;

	private IHistory? RestoreHistory(Bucket bucket) =>
		Exist(bucket, TypeInfo.HistoryNode)
			? new HistoryEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = RestoreIdentifier(bucket.Nested(Key.Id)),
				  Type = bucket.GetEnum(Key.HistoryType).As<HistoryType>(),
				  Transition = RestoreTransition(bucket.Nested(Key.Transition))
			  }
			: null;

	private static IIdentifier? RestoreIdentifier(Bucket bucket)
	{
		var id = bucket.GetString(Key.Id);
		return id is not null ? (Identifier) id : null;
	}

	private IIf? RestoreIf(Bucket bucket) =>
		Exist(bucket, TypeInfo.IfNode)
			? new IfEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Condition = RestoreConditionExpression(bucket.Nested(Key.Condition)),
				  Action = bucket.RestoreList(Key.Action, RestoreExecutableEntity)
			  }
			: null;

	private IInvoke? RestoreInvoke(Bucket bucket) =>
		Exist(bucket, TypeInfo.InvokeNode)
			? new InvokeEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = bucket.GetString(Key.Id),
				  Type = bucket.GetUri(Key.Type),
				  Source = bucket.GetUri(Key.Source),
				  AutoForward = bucket.GetBoolean(Key.AutoForward),
				  TypeExpression = RestoreValueExpression(bucket.Nested(Key.TypeExpression)),
				  SourceExpression = RestoreValueExpression(bucket.Nested(Key.SourceExpression)),
				  IdLocation = RestoreLocationExpression(bucket.Nested(Key.IdLocation)),
				  NameList = bucket.RestoreList(Key.NameList, RestoreLocationExpression),
				  Parameters = bucket.RestoreList(Key.Parameters, RestoreParam),
				  Finalize = RestoreFinalize(bucket.Nested(Key.Finalize)),
				  Content = RestoreContent(bucket.Nested(Key.Content))
			  }
			: null;

	private static ILocationExpression? RestoreLocationExpression(Bucket bucket) =>
		Exist(bucket, TypeInfo.LocationExpressionNode)
			? new LocationExpression
			  {
				  Ancestor = new EntityData(bucket),
				  Expression = bucket.GetString(Key.Expression)
			  }
			: null;

	private IOnEntry? RestoreOnEntry(Bucket bucket) =>
		Exist(bucket, TypeInfo.OnEntryNode)
			? new OnEntryEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Action = bucket.RestoreList(Key.Action, RestoreExecutableEntity)
			  }
			: null;

	private IOnExit? RestoreOnExit(Bucket bucket) =>
		Exist(bucket, TypeInfo.OnExitNode)
			? new OnExitEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Action = bucket.RestoreList(Key.Action, RestoreExecutableEntity)
			  }
			: null;

	private IParallel? RestoreParallel(Bucket bucket) =>
		Exist(bucket, TypeInfo.ParallelNode)
			? new ParallelEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = RestoreIdentifier(bucket.Nested(Key.Id)),
				  DataModel = RestoreDataModel(bucket.Nested(Key.DataModel)),
				  States = bucket.RestoreList(Key.States, RestoreStateEntity),
				  HistoryStates = bucket.RestoreList(Key.HistoryStates, RestoreHistory),
				  Transitions = bucket.RestoreList(Key.Transitions, RestoreTransition),
				  OnEntry = bucket.RestoreList(Key.OnEntry, RestoreOnEntry),
				  OnExit = bucket.RestoreList(Key.OnExit, RestoreOnExit),
				  Invoke = bucket.RestoreList(Key.Invoke, RestoreInvoke)
			  }
			: null;

	private static IParam? RestoreParam(Bucket bucket) =>
		Exist(bucket, TypeInfo.ParamNode)
			? new ParamEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Name = bucket.GetString(Key.Name),
				  Expression = RestoreValueExpression(bucket.Nested(Key.Expression)),
				  Location = RestoreLocationExpression(bucket.Nested(Key.Location))
			  }
			: null;

	private static IScriptExpression? RestoreScriptExpression(Bucket bucket) =>
		Exist(bucket, TypeInfo.ScriptExpressionNode)
			? new ScriptExpression
			  {
				  Ancestor = new EntityData(bucket),
				  Expression = bucket.GetString(Key.Expression)
			  }
			: null;

	private IStateEntity RestoreStateEntity(Bucket bucket)
	{
		var typeInfo = bucket.GetEnum(Key.TypeInfo).As<TypeInfo>();
		return typeInfo switch
			   {
				   TypeInfo.CompoundNode => RestoreCompound(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.FinalNode    => RestoreFinal(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.ParallelNode => RestoreParallel(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   TypeInfo.StateNode    => RestoreState(bucket) ?? throw new PersistenceException(Resources.Exception_CantRestoreElement),
				   _                     => throw new PersistenceException(Resources.Exception_UnknownStateEntityType)
			   };
	}

	private IState? RestoreState(Bucket bucket) =>
		Exist(bucket, TypeInfo.StateNode)
			? new StateEntity
			  {
				  Ancestor = new EntityData(bucket),
				  Id = RestoreIdentifier(bucket.Nested(Key.Id)),
				  Initial = RestoreInitial(bucket.Nested(Key.Initial)),
				  DataModel = RestoreDataModel(bucket.Nested(Key.DataModel)),
				  States = bucket.RestoreList(Key.States, RestoreStateEntity),
				  HistoryStates = bucket.RestoreList(Key.HistoryStates, RestoreHistory),
				  Transitions = bucket.RestoreList(Key.Transitions, RestoreTransition),
				  OnEntry = bucket.RestoreList(Key.OnEntry, RestoreOnEntry),
				  OnExit = bucket.RestoreList(Key.OnExit, RestoreOnExit),
				  Invoke = bucket.RestoreList(Key.Invoke, RestoreInvoke)
			  }
			: null;

	private static IValueExpression? RestoreValueExpression(Bucket bucket) =>
		Exist(bucket, TypeInfo.ValueExpressionNode)
			? new ValueExpression
			  {
				  Ancestor = new EntityData(bucket),
				  Expression = bucket.GetString(Key.Expression)
			  }
			: null;

	private class EntityData : IPersistedDocumentId
	{
		public EntityData(Bucket bucket)
		{
			if (bucket.TryGet(Key.DocumentId, out int documentId))
			{
				DocumentId = documentId;
			}
		}

	#region Interface IPersistedDocumentId

		public int DocumentId { get; }

	#endregion
	}

	private class ExternalScriptExpressionWithContent(object ancestor, Uri? uri, string content) : IExternalScriptExpression, IExternalScriptProvider, IAncestorProvider
	{
	#region Interface IAncestorProvider

		public object Ancestor { get; } = ancestor;

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri { get; } = uri;

	#endregion

	#region Interface IExternalScriptProvider

		public string Content { get; } = content;

	#endregion
	}
}