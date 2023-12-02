#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.Core;

public abstract class StateMachineVisitor
{
	private readonly Stack<(object, ImmutableArray<object?>)>? _path;

	protected StateMachineVisitor(bool trackPath = false) => _path = trackPath ? new Stack<(object, ImmutableArray<object?>)>() : null;

	protected string? CurrentPath => _path is not null ? string.Join(separator: @"/", _path.Reverse().Select(EntityName)) : null;

	private static string EntityName((object obj, ImmutableArray<object?> array) entry)
	{
		if (entry.array.IsDefault)
		{
			return entry.obj.GetType().Name;
		}

		return ((Type) entry.obj).Name;
	}

	private void Enter<T>(T entity) where T : notnull => _path?.Push((entity, new ImmutableArray<object?>()));

	private void Enter<T>(ImmutableArray<T> array) where T : notnull => _path?.Push((typeof(ImmutableArray<T>), array.CastArray<object?>()));

	private void Exit() => _path?.Pop();

	protected void SetRootPath(object root)
	{
		if (root is null) throw new ArgumentNullException(nameof(root));

		if (_path?.Count > 0)
		{
			throw new InvalidOperationException(message: Resources.Exception_RootPathCanBeSetOnlyBeforeVisiting);
		}

		_path?.Push((root, new ImmutableArray<object?>()));
	}

	private ref struct VisitData<TEntity, TIEntity> where TEntity : struct, IVisitorEntity<TEntity, TIEntity>, TIEntity
	{
		private readonly TEntity _original;
		public           TEntity Properties;

		public VisitData(TIEntity entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			if (entity is TEntity tmp)
			{
				_original = tmp;
			}
			else
			{
				_original = new TEntity();
				_original.Init(entity);
			}

			Properties = _original;
		}

		public void Update(ref TIEntity entity)
		{
			if (!_original.RefEquals(ref Properties))
			{
				entity = Properties;
			}
		}
	}

	private ref struct VisitListData<T> where T : class
	{
		private readonly ImmutableArray<T> _original;
		public           TrackList<T>      List;

		public VisitListData(ImmutableArray<T> list)
		{
			if (list.IsDefault)
			{
				throw new ArgumentNullException(nameof(list));
			}

			_original = list;
			List = new TrackList<T>(list);
		}

		public void Update(ref ImmutableArray<T> list)
		{
			if (_original == list && List.IsModified)
			{
				list = List.ModifiedItems!.ToImmutable();
			}
		}
	}

	protected ref struct TrackList<T> where T : class
	{
		private readonly ImmutableArray<T> _items;

		public TrackList(ImmutableArray<T> items)
		{
			_items = items;
			ModifiedItems = default;
		}

		public bool IsModified
		{
			get
			{
				if (ModifiedItems is null)
				{
					return false;
				}

				if (_items.Length != ModifiedItems.Count)
				{
					return true;
				}

				for (var i = 0; i < _items.Length; i ++)
				{
					if (!ReferenceEquals(_items[i], ModifiedItems[i]))
					{
						return true;
					}
				}

				return false;
			}
		}

		public ImmutableArray<T>.Builder? ModifiedItems { get; private set; }

		public readonly int Count => ModifiedItems?.Count ?? _items.Length;

		public T? this[int index]
		{
			get => ModifiedItems is not null ? ModifiedItems[index] : _items[index];
			set
			{
				if (ModifiedItems is not null)
				{
					ModifiedItems[index] = value!;
				}
				else if (!ReferenceEquals(_items[index], value))
				{
					ModifiedItems = _items.ToBuilder();
					ModifiedItems[index] = value!;
				}
			}
		}

		public readonly bool Contains(T item) => ModifiedItems?.Contains(item) ?? _items.Contains(item);

		[MustDisposeResource]
		public readonly IEnumerator<T> GetEnumerator() => ModifiedItems is not null ? ModifiedItems.GetEnumerator() : ((IEnumerable<T>) _items).GetEnumerator();

		public void Add(T? item) => (ModifiedItems ??= _items.ToBuilder()).Add(item!);

		public void Clear()
		{
			if (Count == 0)
			{
				return;
			}

			if (ModifiedItems is null)
			{
				var empty = ImmutableArray<T>.Empty;
				ModifiedItems = empty.ToBuilder();
			}
			else
			{
				ModifiedItems.Clear();
			}
		}

		public void Insert(int index, T item) => (ModifiedItems ??= _items.ToBuilder()).Insert(index, item);

		public void RemoveAt(int index) => (ModifiedItems ??= _items.ToBuilder()).RemoveAt(index);
	}

#region Visit(ref IT entity)

	protected virtual void Visit(ref IIdentifier entity)
	{
		if (entity is null) throw new ArgumentNullException(nameof(entity));
	}

	protected virtual void Visit(ref IEventDescriptor entity)
	{
		if (entity is null) throw new ArgumentNullException(nameof(entity));
	}

	protected virtual void Visit(ref IOutgoingEvent entity)
	{
		if (entity is null) throw new ArgumentNullException(nameof(entity));
	}

	protected virtual void Visit(ref IStateMachine entity)
	{
		var data = new VisitData<StateMachineEntity, IStateMachine>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref ITransition entity)
	{
		var data = new VisitData<TransitionEntity, ITransition>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IState entity)
	{
		var data = new VisitData<StateEntity, IState>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IParallel entity)
	{
		var data = new VisitData<ParallelEntity, IParallel>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IInitial entity)
	{
		var data = new VisitData<InitialEntity, IInitial>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IFinal entity)
	{
		var data = new VisitData<FinalEntity, IFinal>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IHistory entity)
	{
		var data = new VisitData<HistoryEntity, IHistory>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IOnEntry entity)
	{
		var data = new VisitData<OnEntryEntity, IOnEntry>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IOnExit entity)
	{
		var data = new VisitData<OnExitEntity, IOnExit>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IAssign entity)
	{
		var data = new VisitData<AssignEntity, IAssign>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref ICancel entity)
	{
		var data = new VisitData<CancelEntity, ICancel>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IElseIf entity)
	{
		var data = new VisitData<ElseIfEntity, IElseIf>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IForEach entity)
	{
		var data = new VisitData<ForEachEntity, IForEach>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IIf entity)
	{
		var data = new VisitData<IfEntity, IIf>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref ILog entity)
	{
		var data = new VisitData<LogEntity, ILog>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IRaise entity)
	{
		var data = new VisitData<RaiseEntity, IRaise>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IScript entity)
	{
		var data = new VisitData<ScriptEntity, IScript>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref ICustomAction entity)
	{
		var data = new VisitData<CustomActionEntity, ICustomAction>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref ISend entity)
	{
		var data = new VisitData<SendEntity, ISend>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IDataModel entity)
	{
		var data = new VisitData<DataModelEntity, IDataModel>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IData entity)
	{
		var data = new VisitData<DataEntity, IData>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IDoneData entity)
	{
		var data = new VisitData<DoneDataEntity, IDoneData>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IInvoke entity)
	{
		var data = new VisitData<InvokeEntity, IInvoke>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IContent entity)
	{
		var data = new VisitData<ContentEntity, IContent>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IContentBody entity)
	{
		var data = new VisitData<ContentBody, IContentBody>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IInlineContent entity)
	{
		var data = new VisitData<InlineContent, IInlineContent>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IParam entity)
	{
		var data = new VisitData<ParamEntity, IParam>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IFinalize entity)
	{
		var data = new VisitData<FinalizeEntity, IFinalize>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IElse entity)
	{
		var data = new VisitData<ElseEntity, IElse>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IValueExpression entity)
	{
		var data = new VisitData<ValueExpression, IValueExpression>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref ILocationExpression entity)
	{
		var data = new VisitData<LocationExpression, ILocationExpression>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IConditionExpression entity)
	{
		var data = new VisitData<ConditionExpression, IConditionExpression>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IScriptExpression entity)
	{
		var data = new VisitData<ScriptExpression, IScriptExpression>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IExternalScriptExpression entity)
	{
		var data = new VisitData<ExternalScriptExpression, IExternalScriptExpression>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IExternalDataExpression entity)
	{
		var data = new VisitData<ExternalDataExpression, IExternalDataExpression>(entity);
		Build(ref data.Properties);
		data.Update(ref entity);
	}

	protected virtual void Visit(ref IExecutableEntity entity)
	{
		if (entity is null) throw new ArgumentNullException(nameof(entity));

		switch (entity)
		{
			case IAssign assign:
				Visit(ref assign);
				entity = assign;
				break;

			case ICancel cancel:
				Visit(ref cancel);
				entity = cancel;
				break;

			case ICustomAction customAction:
				Visit(ref customAction);
				entity = customAction;
				break;

			case IElse @else:
				Visit(ref @else);
				entity = @else;
				break;

			case IElseIf elseIf:
				Visit(ref elseIf);
				entity = elseIf;
				break;

			case IForEach forEach:
				Visit(ref forEach);
				entity = forEach;
				break;

			case IIf @if:
				Visit(ref @if);
				entity = @if;
				break;

			case ILog log:
				Visit(ref log);
				entity = log;
				break;

			case IRaise raise:
				Visit(ref raise);
				entity = raise;
				break;

			case IScript script:
				Visit(ref script);
				entity = script;
				break;

			case ISend send:
				Visit(ref send);
				entity = send;
				break;

			default:
				VisitUnknown(ref entity);
				break;
		}
	}

	protected virtual void VisitUnknown(ref IExecutableEntity entity) { }

	protected virtual void Visit(ref IStateEntity entity)
	{
		if (entity is null) throw new ArgumentNullException(nameof(entity));

		switch (entity)
		{
			case IState state:
				Visit(ref state);
				entity = state;
				break;

			case IParallel parallel:
				Visit(ref parallel);
				entity = parallel;
				break;

			case IFinal final:
				Visit(ref final);
				entity = final;
				break;

			default:
				VisitUnknown(ref entity);
				break;
		}
	}

	protected virtual void VisitUnknown(ref IStateEntity entity) { }

#endregion

#region Visit(ref ImmutableArray<IT> list)

	protected virtual void Visit(ref ImmutableArray<IIdentifier> list)
	{
		var data = new VisitListData<IIdentifier>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IStateEntity> list)
	{
		var data = new VisitListData<IStateEntity>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<ITransition> list)
	{
		var data = new VisitListData<ITransition>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IEventDescriptor> list)
	{
		var data = new VisitListData<IEventDescriptor>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IHistory> list)
	{
		var data = new VisitListData<IHistory>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IOnEntry> list)
	{
		var data = new VisitListData<IOnEntry>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IOnExit> list)
	{
		var data = new VisitListData<IOnExit>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IExecutableEntity> list)
	{
		var data = new VisitListData<IExecutableEntity>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IInvoke> list)
	{
		var data = new VisitListData<IInvoke>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IValueExpression> list)
	{
		var data = new VisitListData<IValueExpression>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<ILocationExpression> list)
	{
		var data = new VisitListData<ILocationExpression>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IData> list)
	{
		var data = new VisitListData<IData>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

	protected virtual void Visit(ref ImmutableArray<IParam> list)
	{
		var data = new VisitListData<IParam>(list);
		Build(ref data.List);
		data.Update(ref list);
	}

#endregion

#region Build(ref IT entity, ref T properties)

	protected virtual void Build(ref StateMachineEntity properties)
	{
		var dataModel = properties.DataModel;
		VisitWrapper(ref dataModel);
		properties.DataModel = dataModel;

		var script = properties.Script;
		VisitWrapper(ref script);
		properties.Script = script;

		var initial = properties.Initial;
		VisitWrapper(ref initial);
		properties.Initial = initial;

		var states = properties.States;
		VisitWrapper(ref states);
		properties.States = states;
	}

	protected virtual void Build(ref TransitionEntity properties)
	{
		var evt = properties.EventDescriptors;
		VisitWrapper(ref evt);
		properties.EventDescriptors = evt;

		var target = properties.Target;
		VisitWrapper(ref target);
		properties.Target = target;

		var condition = properties.Condition;
		VisitWrapper(ref condition);
		properties.Condition = condition;

		var action = properties.Action;
		VisitWrapper(ref action);
		properties.Action = action;
	}

	protected virtual void Build(ref StateEntity properties)
	{
		var id = properties.Id;
		VisitWrapper(ref id);
		properties.Id = id;

		var dataModel = properties.DataModel;
		VisitWrapper(ref dataModel);
		properties.DataModel = dataModel;

		var initial = properties.Initial;
		VisitWrapper(ref initial);
		properties.Initial = initial;

		var states = properties.States;
		VisitWrapper(ref states);
		properties.States = states;

		var historyStates = properties.HistoryStates;
		VisitWrapper(ref historyStates);
		properties.HistoryStates = historyStates;

		var transitions = properties.Transitions;
		VisitWrapper(ref transitions);
		properties.Transitions = transitions;

		var onEntry = properties.OnEntry;
		VisitWrapper(ref onEntry);
		properties.OnEntry = onEntry;

		var onExit = properties.OnExit;
		VisitWrapper(ref onExit);
		properties.OnExit = onExit;

		var invoke = properties.Invoke;
		VisitWrapper(ref invoke);
		properties.Invoke = invoke;
	}

	protected virtual void Build(ref ParallelEntity properties)
	{
		var id = properties.Id;
		VisitWrapper(ref id);
		properties.Id = id;

		var dataModel = properties.DataModel;
		VisitWrapper(ref dataModel);
		properties.DataModel = dataModel;

		var states = properties.States;
		VisitWrapper(ref states);
		properties.States = states;

		var historyStates = properties.HistoryStates;
		VisitWrapper(ref historyStates);
		properties.HistoryStates = historyStates;

		var transitions = properties.Transitions;
		VisitWrapper(ref transitions);
		properties.Transitions = transitions;

		var onEntry = properties.OnEntry;
		VisitWrapper(ref onEntry);
		properties.OnEntry = onEntry;

		var onExit = properties.OnExit;
		VisitWrapper(ref onExit);
		properties.OnExit = onExit;

		var invoke = properties.Invoke;
		VisitWrapper(ref invoke);
		properties.Invoke = invoke;
	}

	protected virtual void Build(ref InitialEntity properties)
	{
		var transition = properties.Transition;
		VisitWrapper(ref transition);
		properties.Transition = transition;
	}

	protected virtual void Build(ref FinalEntity properties)
	{
		var id = properties.Id;
		VisitWrapper(ref id);
		properties.Id = id;

		var onEntry = properties.OnEntry;
		VisitWrapper(ref onEntry);
		properties.OnEntry = onEntry;

		var onExit = properties.OnExit;
		VisitWrapper(ref onExit);
		properties.OnExit = onExit;

		var doneData = properties.DoneData;
		VisitWrapper(ref doneData);
		properties.DoneData = doneData;
	}

	protected virtual void Build(ref HistoryEntity properties)
	{
		var id = properties.Id;
		VisitWrapper(ref id);
		properties.Id = id;

		var transition = properties.Transition;
		VisitWrapper(ref transition);
		properties.Transition = transition;
	}

	protected virtual void Build(ref OnEntryEntity properties)
	{
		var action = properties.Action;
		VisitWrapper(ref action);
		properties.Action = action;
	}

	protected virtual void Build(ref OnExitEntity properties)
	{
		var action = properties.Action;
		VisitWrapper(ref action);
		properties.Action = action;
	}

	protected virtual void Build(ref AssignEntity properties)
	{
		var location = properties.Location;
		VisitWrapper(ref location!);
		properties.Location = location;

		var expression = properties.Expression;
		VisitWrapper(ref expression);
		properties.Expression = expression;

		var inlineContent = properties.InlineContent;
		VisitWrapper(ref inlineContent);
		properties.InlineContent = inlineContent;
	}

	protected virtual void Build(ref CancelEntity properties)
	{
		var sendIdExpression = properties.SendIdExpression;
		VisitWrapper(ref sendIdExpression);
		properties.SendIdExpression = sendIdExpression;
	}

	protected virtual void Build(ref ElseIfEntity properties)
	{
		var condition = properties.Condition;
		VisitWrapper(ref condition);
		properties.Condition = condition;
	}

	protected virtual void Build(ref ForEachEntity properties)
	{
		var array = properties.Array;
		VisitWrapper(ref array!);
		properties.Array = array;

		var item = properties.Item;
		VisitWrapper(ref item!);
		properties.Item = item;

		var index = properties.Index;
		VisitWrapper(ref index);
		properties.Index = index;

		var action = properties.Action;
		VisitWrapper(ref action);
		properties.Action = action;
	}

	protected virtual void Build(ref IfEntity properties)
	{
		var condition = properties.Condition;
		VisitWrapper(ref condition!);
		properties.Condition = condition;

		var action = properties.Action;
		VisitWrapper(ref action);
		properties.Action = action;
	}

	protected virtual void Build(ref LogEntity properties)
	{
		var expression = properties.Expression;
		VisitWrapper(ref expression);
		properties.Expression = expression;
	}

	protected virtual void Build(ref RaiseEntity properties)
	{
		var evt = properties.OutgoingEvent;
		VisitWrapper(ref evt);
		properties.OutgoingEvent = evt;
	}

	protected virtual void Build(ref ScriptEntity properties)
	{
		var content = properties.Content;
		VisitWrapper(ref content);
		properties.Content = content;

		var source = properties.Source;
		VisitWrapper(ref source);
		properties.Source = source;
	}

	protected virtual void Build(ref SendEntity properties)
	{
		var eventExpression = properties.EventExpression;
		VisitWrapper(ref eventExpression);
		properties.EventExpression = eventExpression;

		var targetExpression = properties.TargetExpression;
		VisitWrapper(ref targetExpression);
		properties.TargetExpression = targetExpression;

		var typeExpression = properties.TypeExpression;
		VisitWrapper(ref typeExpression);
		properties.TypeExpression = typeExpression;

		var idLocation = properties.IdLocation;
		VisitWrapper(ref idLocation);
		properties.IdLocation = idLocation;

		var delayExpression = properties.DelayExpression;
		VisitWrapper(ref delayExpression);
		properties.DelayExpression = delayExpression;

		var nameList = properties.NameList;
		VisitWrapper(ref nameList);
		properties.NameList = nameList;

		var parameters = properties.Parameters;
		VisitWrapper(ref parameters);
		properties.Parameters = parameters;

		var content = properties.Content;
		VisitWrapper(ref content);
		properties.Content = content;
	}

	protected virtual void Build(ref DataModelEntity properties)
	{
		var data = properties.Data;
		VisitWrapper(ref data);
		properties.Data = data;
	}

	protected virtual void Build(ref DataEntity properties)
	{
		var expression = properties.Expression;
		VisitWrapper(ref expression);
		properties.Expression = expression;

		var inlineContent = properties.InlineContent;
		VisitWrapper(ref inlineContent);
		properties.InlineContent = inlineContent;

		var source = properties.Source;
		VisitWrapper(ref source);
		properties.Source = source;
	}

	protected virtual void Build(ref DoneDataEntity properties)
	{
		var content = properties.Content;
		VisitWrapper(ref content);
		properties.Content = content;

		var parameters = properties.Parameters;
		VisitWrapper(ref parameters);
		properties.Parameters = parameters;
	}

	protected virtual void Build(ref InvokeEntity properties)
	{
		var typeExpression = properties.TypeExpression;
		VisitWrapper(ref typeExpression);
		properties.TypeExpression = typeExpression;

		var sourceExpression = properties.SourceExpression;
		VisitWrapper(ref sourceExpression);
		properties.SourceExpression = sourceExpression;

		var idLocation = properties.IdLocation;
		VisitWrapper(ref idLocation);
		properties.IdLocation = idLocation;

		var nameList = properties.NameList;
		VisitWrapper(ref nameList);
		properties.NameList = nameList;

		var content = properties.Content;
		VisitWrapper(ref content);
		properties.Content = content;

		var parameters = properties.Parameters;
		VisitWrapper(ref parameters);
		properties.Parameters = parameters;

		var finalize = properties.Finalize;
		VisitWrapper(ref finalize);
		properties.Finalize = finalize;
	}

	protected virtual void Build(ref ContentEntity properties)
	{
		var expression = properties.Expression;
		VisitWrapper(ref expression);
		properties.Expression = expression;

		var body = properties.Body;
		VisitWrapper(ref body);
		properties.Body = body;
	}

	protected virtual void Build(ref ParamEntity properties)
	{
		var expression = properties.Expression;
		VisitWrapper(ref expression);
		properties.Expression = expression;

		var location = properties.Location;
		VisitWrapper(ref location);
		properties.Location = location;
	}

	protected virtual void Build(ref FinalizeEntity properties)
	{
		var action = properties.Action;
		VisitWrapper(ref action);
		properties.Action = action;
	}

	protected virtual void Build(ref CustomActionEntity properties)
	{
		var values = properties.Values;
		VisitWrapper(ref values);
		properties.Values = values;

		var locations = properties.Locations;
		VisitWrapper(ref locations);
		properties.Locations = locations;
	}

	protected virtual void Build(ref ElseEntity properties) { }

	protected virtual void Build(ref ValueExpression properties) { }

	protected virtual void Build(ref LocationExpression properties) { }

	protected virtual void Build(ref ConditionExpression properties) { }

	protected virtual void Build(ref ScriptExpression properties) { }

	protected virtual void Build(ref ExternalScriptExpression properties) { }

	protected virtual void Build(ref ExternalDataExpression properties) { }

	protected virtual void Build(ref ContentBody properties) { }

	protected virtual void Build(ref InlineContent properties) { }

#endregion

#region Build(ref ImmutableArray<IT> list, ref TrackList<IT> trackList)

	protected virtual void Build(ref TrackList<IIdentifier> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IStateEntity> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<ITransition> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IEventDescriptor> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IHistory> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IOnEntry> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IOnExit> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IExecutableEntity> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IInvoke> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IValueExpression> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<ILocationExpression> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IData> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

	protected virtual void Build(ref TrackList<IParam> trackList)
	{
		for (var i = 0; i < trackList.Count; i ++)
		{
			var item = trackList[i];
			VisitWrapper(ref item);
			trackList[i] = item;
		}
	}

#endregion

#region VisitWrapper(ref IT entity)

	private void VisitWrapper(ref IExecutableEntity? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IInitial? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IIdentifier? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IStateEntity? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IEventDescriptor? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ITransition? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IDoneData? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IHistory? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IOnEntry? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IOnExit? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IConditionExpression? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ILocationExpression? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IValueExpression? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IExternalDataExpression? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IScriptExpression? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IExternalScriptExpression? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IOutgoingEvent? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IInvoke? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IDataModel? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IData? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IParam? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IContent? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IContentBody? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IInlineContent? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref IFinalize? entity)
	{
		if (entity is null) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

#endregion

#region VisitWrapper(ref ImmutableArray<IT> entity)

	private void VisitWrapper(ref ImmutableArray<IStateEntity> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<ITransition> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IEventDescriptor> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IIdentifier> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IExecutableEntity> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IHistory> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IOnEntry> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IOnExit> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IInvoke> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IValueExpression> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<ILocationExpression> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IData> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

	private void VisitWrapper(ref ImmutableArray<IParam> entity)
	{
		if (entity.IsDefault) return;
		Enter(entity);
		Visit(ref entity);
		Exit();
	}

#endregion
}