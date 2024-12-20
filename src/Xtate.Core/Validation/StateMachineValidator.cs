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

namespace Xtate.Core;

public class StateMachineValidator : StateMachineVisitor, IStateMachineValidator
{
	public required IErrorProcessorService<StateMachineValidator> ErrorProcessorService { private get; [UsedImplicitly] init; }

#region Interface IStateMachineValidator

	public virtual void Validate(IStateMachine stateMachine) => Visit(ref stateMachine);

#endregion

	protected override void Visit(ref IAssign entity)
	{
		if (entity.Location is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_AssignItemLocationMissed);
		}

		if (entity.Expression is null && entity.InlineContent is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_AssignItemContentAndExpressionMissed);
		}

		if (entity.Expression is not null && entity.InlineContent is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_AssignItemContentAndExpressionSpecified);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref ICancel entity)
	{
		if (entity.SendId is null && entity.SendIdExpression is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_CancelItemSendIdAndExpressionMissed);
		}

		if (entity.SendId is not null && entity.SendIdExpression is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_CancelItemSendIdAndExpressionSpecified);
		}

		if (entity.SendId is { Length: 0 })
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_SendidAttributeCantBeEmptyInCancelElement);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IConditionExpression entity)
	{
		if (entity.Expression is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ConditionExpressionCantBeNull);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref ILocationExpression entity)
	{
		if (entity.Expression is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_LocationExpressionCantBeNull);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IScriptExpression entity)
	{
		if (entity.Expression is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ScriptExpressionCantBeNull);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IContentBody entity)
	{
		if (entity.Value is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ContentValueCantBeNull);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IInlineContent entity)
	{
		if (entity.Value is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ContentValueCantBeNull);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IContent entity)
	{
		if (entity.Expression is null && entity.Body is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ExpressionAndBodyMissedInContent);
		}

		if (entity.Expression is not null && entity.Body is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ExpressionAndBodySpecifiedInContent);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref ICustomAction entity)
	{
		if (entity.Xml is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_XmlCannotBeNull);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IData entity)
	{
		if (string.IsNullOrEmpty(entity.Id))
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_IdPropertyRequiredInDataElement);
		}

		if ((entity.InlineContent is not null && entity.Expression is not null) ||
			(entity.InlineContent is not null && entity.Source is not null) ||
			(entity.Source is not null && entity.Expression is not null))
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ExpressionSourceInData);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IElseIf entity)
	{
		if (entity.Condition is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ConditionRequiredForElseIf);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IFinalize entity)
	{
		foreach (var executableEntity in entity.Action)
		{
			if (executableEntity is IRaise)
			{
				ErrorProcessorService.AddError(executableEntity, Resources.ErrorMessage_RaiseCantBeUsedInFinalizeElement);
			}

			if (executableEntity is ISend)
			{
				ErrorProcessorService.AddError(executableEntity, Resources.ErrorMessage_SendCantBeUsedInFinalizeElement);
			}
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IForEach entity)
	{
		if (entity.Array is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ArrayPropertyRequiredForForEach);
		}

		if (entity.Item is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ConditionRequiredForForEach);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IHistory entity)
	{
		if (entity.Transition is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_TransitionMustBePresentInHistoryElement);
		}

		if (entity.Type is < HistoryType.Shallow or > HistoryType.Deep)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_InvalidTypeValueInHistoryElement);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IIf entity)
	{
		if (entity.Condition is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ConditionRequiredForIf);
		}

		var condition = true;

		foreach (var op in entity.Action)
		{
			switch (op)
			{
				case IElseIf:
					if (!condition)
					{
						ErrorProcessorService.AddError(op, Resources.ErrorMessage_ElseifCannotFollowElse);
					}

					break;

				case IElse:
					if (!condition)
					{
						ErrorProcessorService.AddError(op, Resources.ErrorMessage_ElseCanBeUsedOnlyOnce);
					}

					condition = false;

					break;
			}
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IInitial entity)
	{
		if (entity.Transition is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_TransitionMustBePresentInInitialElement);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IInvoke entity)
	{
		if (entity.Type is null && entity.TypeExpression is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_TypeOrTypeExpressionMustBeSpecifiedInInvokeElement);
		}

		if (entity.Type is not null && entity.TypeExpression is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_TypeAndTypeExpressionCantBeUsedAtTheSameTimeInInvokeElement);
		}

		if (entity.Id is not null && entity.IdLocation is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_IdAndIdLocationCantBeUsedAtTheSameTimeInInvokeElement);
		}

		if (entity.Source is not null && entity.SourceExpression is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_SourceAndSourceExpressionCantBeUsedAtTheSameTimeInInvokeElement);
		}

		if (!entity.NameList.IsDefaultOrEmpty && !entity.Parameters.IsDefaultOrEmpty)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_NameListAndParametersCantBeUsedAtTheSameTimeInInvokeElement);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IParam entity)
	{
		if (entity.Name is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_NameAttributesRequiredInParamElement);
		}

		if (entity.Expression is not null && entity.Location is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ExpressionLocationInParam);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IRaise entity)
	{
		if (entity.OutgoingEvent is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_EventRequiredForRaise);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IScript entity)
	{
		if (entity.Source is not null && entity.Content is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_SourceAndBodyCantBeUsedAtTheSameTimeInAssignElement);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref ISend entity)
	{
		if ((entity.EventName is not null && entity.EventExpression is not null) ||
			(entity.EventName is not null && entity.Content is not null) ||
			(entity.EventExpression is not null && entity.Content is not null))
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_EvenExpressionContentInSend);
		}

		if (entity.Target is not null && entity.TargetExpression is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_TargetAndTargetExpressionCantBeUsedAtTheSameTimeInSendElement);
		}

		if (entity.Type is not null && entity.TypeExpression is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_TypeAndTypeExpressionCantBeUsedAtTheSameTimeInSendElement);
		}

		if (entity.Id is not null && entity.IdLocation is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_IdAndIdLocationCantBeUsedAtTheSameTimeInSendElement);
		}

		if (entity.DelayMs is not null && entity.DelayExpression is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_EventExpressionInSend);
		}

		if (!entity.NameList.IsDefaultOrEmpty && entity.Content is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_NameListAndContentCantBeUsedAtTheSameTimeInSendElement);
		}

		if (!entity.Parameters.IsDefaultOrEmpty && entity.Content is not null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_ParametersAndContentCantBeUsedAtTheSameTimeInSendElement);
		}

		if (entity.EventName is null && entity.EventExpression is null && entity.Content is null)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_MustBePresentEventOrEventExpressionOrContentInSendElement);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IStateMachine entity)
	{
		if (entity.Initial is not null && entity.States.IsDefaultOrEmpty)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_InitialStatePropertyCannotBeUsedWithoutAnyStates);
		}

		if (entity.Binding is < BindingType.Early or > BindingType.Late)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_InvalidBindingTypeValueInStateMachineElement);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref IState entity)
	{
		if (entity.Initial is not null && entity.States.IsDefaultOrEmpty)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_InitialStatePropertyCanBeUsedOnlyInComplexStates);
		}

		base.Visit(ref entity);
	}

	protected override void Visit(ref ITransition entity)
	{
		if (entity.EventDescriptors.IsDefault && entity.Condition is null && entity.Target.IsDefault)
		{
			ErrorProcessorService.AddError(entity, Resources.ErrorMessage_MustBePresentAtLeastEventOrConditionOrTargetInTransitionElement);
		}

		base.Visit(ref entity);
	}
}