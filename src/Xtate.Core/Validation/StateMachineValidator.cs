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

namespace Xtate
{
	public class StateMachineValidator : IStateMachineValidator
	{
		public static IStateMachineValidator Instance { get; } = new StateMachineValidator();

	#region Interface IStateMachineValidator

		public void Validate(IStateMachine stateMachine, IErrorProcessor errorProcessor)
		{
			new Validator(errorProcessor).Validate(stateMachine);
		}

	#endregion

		private class Validator : StateMachineVisitor
		{
			private readonly IErrorProcessor _errorProcessor;

			public Validator(IErrorProcessor errorProcessor) => _errorProcessor = errorProcessor;

			public void Validate(IStateMachine stateMachine)
			{
				Visit(ref stateMachine);
			}

			private void AddError(object entity, string message) => _errorProcessor.AddError<StateMachineValidator>(entity, message);

			protected override void Visit(ref IAssign entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Location is null)
				{
					AddError(entity, Resources.ErrorMessage_AssignItemLocationMissed);
				}

				if (entity.Expression is null && entity.InlineContent is null)
				{
					AddError(entity, Resources.ErrorMessage_AssignItemContentAndExpressionMissed);
				}

				if (entity.Expression is not null && entity.InlineContent is not null)
				{
					AddError(entity, Resources.ErrorMessage_AssignItemContentAndExpressionSpecified);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ICancel entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.SendId is null && entity.SendIdExpression is null)
				{
					AddError(entity, Resources.ErrorMessage_CancelItemSendIdAndExpressionMissed);
				}

				if (entity.SendId is not null && entity.SendIdExpression is not null)
				{
					AddError(entity, Resources.ErrorMessage_CancelItemSendIdAndExpressionSpecified);
				}

				if (entity.SendId is { Length: 0 })
				{
					AddError(entity, Resources.ErrorMessage_SendidAttributeCantBeEmptyInCancelElement);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IConditionExpression entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression is null)
				{
					AddError(entity, Resources.ErrorMessage_ConditionExpressionCantBeNull);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ILocationExpression entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression is null)
				{
					AddError(entity, Resources.ErrorMessage_Location_expression_can_t_be_null);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IScriptExpression entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression is null)
				{
					AddError(entity, Resources.ErrorMessage_Script_expression_can_t_be_null);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IContentBody entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Value is null)
				{
					AddError(entity, Resources.ErrorMessage_ContentValueCantBeNull);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IInlineContent entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Value is null)
				{
					AddError(entity, Resources.ErrorMessage_ContentValueCantBeNull);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IContent entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression is null && entity.Body is null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionAndBodyMissedInContent);
				}

				if (entity.Expression is not null && entity.Body is not null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionAndBodySpecifiedInContent);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ICustomAction entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Xml is null)
				{
					AddError(entity, Resources.ErrorMessage_XmlCannotBeNull);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IData entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (string.IsNullOrEmpty(entity.Id))
				{
					AddError(entity, Resources.ErrorMessage_Id_property_required_in_Data_element);
				}

				if (entity.InlineContent is not null && entity.Expression is not null || entity.InlineContent is not null && entity.Source is not null ||
					entity.Source is not null && entity.Expression is not null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionSourceInData);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IElseIf entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Condition is null)
				{
					AddError(entity, Resources.ErrorMessage_ConditionRequiredForElseIf);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IFinalize entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				foreach (var executableEntity in entity.Action)
				{
					if (executableEntity is IRaise)
					{
						AddError(executableEntity, Resources.ErrorMessage_Raise_can_t_be_used_in_Finalize_element);
					}

					if (executableEntity is ISend)
					{
						AddError(executableEntity, Resources.ErrorMessage_Send_can_t_be_used_in_Finalize_element);
					}
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IForEach entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Array is null)
				{
					AddError(entity, Resources.ErrorMessage_ArrayPropertyRequiredForForEach);
				}

				if (entity.Item is null)
				{
					AddError(entity, Resources.ErrorMessage_ConditionRequiredForForEach);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IHistory entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Transition is null)
				{
					AddError(entity, Resources.ErrorMessage_Transition_must_be_present_in_History_element);
				}

				if (entity.Type < HistoryType.Shallow || entity.Type > HistoryType.Deep)
				{
					AddError(entity, Resources.ErrorMessage_Invalid_Type_value_in_History_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IIf entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Condition is null)
				{
					AddError(entity, Resources.ErrorMessage_onditionRequiredForIf);
				}

				var condition = true;

				foreach (var op in entity.Action)
				{
					switch (op)
					{
						case IElseIf:
							if (!condition)
							{
								AddError(op, Resources.ErrorMessage_ElseifCannotFollowElse);
							}

							break;

						case IElse:
							if (!condition)
							{
								AddError(op, Resources.ErrorMessage_ElseCanBeUsedOnlyOnce);
							}

							condition = false;
							break;
					}
				}


				base.Visit(ref entity);
			}

			protected override void Visit(ref IInitial entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Transition is null)
				{
					AddError(entity, Resources.ErrorMessage_Transition_must_be_present_in_Initial_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IInvoke entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Type is null && entity.TypeExpression is null)
				{
					AddError(entity, Resources.ErrorMessage_Type_or_TypeExpression_must_be_specified_in_Invoke_element);
				}

				if (entity.Type is not null && entity.TypeExpression is not null)
				{
					AddError(entity, Resources.ErrorMessage_Type_and_TypeExpression_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				if (entity.Id is not null && entity.IdLocation is not null)
				{
					AddError(entity, Resources.ErrorMessage_Id_and_IdLocation_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				if (entity.Source is not null && entity.SourceExpression is not null)
				{
					AddError(entity, Resources.ErrorMessage_Source_and_SourceExpression_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				if (!entity.NameList.IsDefaultOrEmpty && !entity.Parameters.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_NameList_and_Parameters_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IParam entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Name is null)
				{
					AddError(entity, Resources.ErrorMessage_Name_attributes_required_in_Param_element);
				}

				if (entity.Expression is not null && entity.Location is not null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionLocationInParam);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IRaise entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.OutgoingEvent is null)
				{
					AddError(entity, Resources.ErrorMessage_EventRequiredForRaise);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IScript entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Source is not null && entity.Content is not null)
				{
					AddError(entity, Resources.ErrorMessage_Source_and_Body_can_t_be_used_at_the_same_time_in_Assign_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ISend entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.EventName is not null && entity.EventExpression is not null ||
					entity.EventName is not null && entity.Content is not null ||
					entity.EventExpression is not null && entity.Content is not null)
				{
					AddError(entity, Resources.ErrorMessage_EvenExpressionContentInSend);
				}

				if (entity.Target is not null && entity.TargetExpression is not null)
				{
					AddError(entity, Resources.ErrorMessage_Target_and_TargetExpression_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.Type is not null && entity.TypeExpression is not null)
				{
					AddError(entity, Resources.ErrorMessage_Type_and_TypeExpression_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.Id is not null && entity.IdLocation is not null)
				{
					AddError(entity, Resources.ErrorMessage_Id_and_IdLocation_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.DelayMs is not null && entity.DelayExpression is not null)
				{
					AddError(entity, Resources.ErrorMessage_EventExpressionInSend);
				}

				if (!entity.NameList.IsDefaultOrEmpty && entity.Content is not null)
				{
					AddError(entity, Resources.ErrorMessage_NameList_and_Content_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (!entity.Parameters.IsDefaultOrEmpty && entity.Content is not null)
				{
					AddError(entity, Resources.ErrorMessage_Parameters_and_Content_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.EventName is null && entity.EventExpression is null && entity.Content is null)
				{
					AddError(entity, Resources.ErrorMessage_Must_be_present_Event_or_EventExpression_or_Content_in_Send_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IStateMachine entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Initial is not null && entity.States.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_Initial_state_property_cannot_be_used_without_any_states);
				}

				if (entity.Binding < BindingType.Early || entity.Binding > BindingType.Late)
				{
					AddError(entity, Resources.ErrorMessage_Invalid_BindingType_value_in_StateMachine_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IState entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.Initial is not null && entity.States.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_Initial_state_property_can_be_used_only_in_complex_states);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ITransition entity)
			{
				if (entity is null) throw new ArgumentNullException(nameof(entity));

				if (entity.EventDescriptors.IsDefaultOrEmpty && entity.Condition is null && entity.Target.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_Must_be_present_at_least_Event_or_Condition_or_Target_in_Transition_element);
				}

				base.Visit(ref entity);
			}
		}
	}
}