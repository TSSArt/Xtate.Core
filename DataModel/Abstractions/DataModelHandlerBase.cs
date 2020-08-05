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
using System.Collections.Immutable;

namespace Xtate.DataModel
{
	public abstract class DataModelHandlerBase : StateMachineVisitor, IDataModelHandler
	{
		private readonly IErrorProcessor _errorProcessor;

		protected DataModelHandlerBase(IErrorProcessor errorProcessor) => _errorProcessor = errorProcessor;

	#region Interface IDataModelHandler

		public virtual void ExecutionContextCreated(IExecutionContext executionContext, out ImmutableDictionary<string, string> dataModelVars)
		{
			dataModelVars = ImmutableDictionary<string, string>.Empty;
		}

		void IDataModelHandler.Process(ref IExecutableEntity executableEntity)
		{
			Visit(ref executableEntity);
		}

		void IDataModelHandler.Process(ref IDataModel dataModel)
		{
			Visit(ref dataModel);
		}

		void IDataModelHandler.Process(ref IDoneData doneData)
		{
			Visit(ref doneData);
		}

		void IDataModelHandler.Process(ref IInvoke invoke)
		{
			Visit(ref invoke);
		}

		public virtual bool CaseInsensitive => false;

	#endregion

		protected void AddErrorMessage(object? entity, string message, Exception? exception = default) => _errorProcessor.AddError(GetType(), entity, message, exception);

		protected override void Visit(ref IValueExpression expression)
		{
			base.Visit(ref expression);

			if (!(expression is IValueEvaluator))
			{
				AddErrorMessage(expression, Resources.ErrorMessage_Value_expression__does_not_implement_IValueEvaluator_);
			}
		}

		protected override void Visit(ref ILocationExpression expression)
		{
			base.Visit(ref expression);

			if (!(expression is ILocationEvaluator))
			{
				AddErrorMessage(expression, Resources.ErrorMessage_Location_expression__does_not_implement_ILocationEvaluator);
			}
		}

		protected override void Build(ref ILog log, ref LogEntity logProperties)
		{
			base.Build(ref log, ref logProperties);

			log = new DefaultLogEvaluator(logProperties);
		}

		protected override void Build(ref ISend send, ref SendEntity sendProperties)
		{
			base.Build(ref send, ref sendProperties);

			send = new DefaultSendEvaluator(sendProperties);
		}

		protected override void Build(ref IDoneData doneData, ref DoneDataEntity doneDataProperties)
		{
			base.Build(ref doneData, ref doneDataProperties);

			doneData = new DefaultDoneDataEvaluator(doneDataProperties);
		}

		protected override void Build(ref IParam param, ref ParamEntity paramProperties)
		{
			base.Build(ref param, ref paramProperties);

			param = new DefaultParam(paramProperties);
		}

		protected override void Build(ref ICancel cancel, ref CancelEntity cancelProperties)
		{
			base.Build(ref cancel, ref cancelProperties);

			cancel = new DefaultCancelEvaluator(cancelProperties);
		}

		protected override void Build(ref IIf @if, ref IfEntity ifProperties)
		{
			base.Build(ref @if, ref ifProperties);

			@if = new DefaultIfEvaluator(ifProperties);
		}

		protected override void Build(ref IRaise raise, ref RaiseEntity raiseProperties)
		{
			base.Build(ref raise, ref raiseProperties);

			raise = new DefaultRaiseEvaluator(raiseProperties);
		}

		protected override void Build(ref IForEach forEach, ref ForEachEntity forEachProperties)
		{
			base.Build(ref forEach, ref forEachProperties);

			forEach = new DefaultForEachEvaluator(forEachProperties);
		}

		protected override void Build(ref IAssign assign, ref AssignEntity assignProperties)
		{
			base.Build(ref assign, ref assignProperties);

			assign = new DefaultAssignEvaluator(assignProperties);
		}

		protected override void Build(ref IScript script, ref ScriptEntity scriptProperties)
		{
			base.Build(ref script, ref scriptProperties);

			script = new DefaultScriptEvaluator(scriptProperties);
		}

		protected override void Build(ref ICustomAction customAction, ref CustomActionEntity customActionProperties)
		{
			base.Build(ref customAction, ref customActionProperties);

			customAction = new DefaultCustomActionEvaluator(customActionProperties);
		}

		protected override void Build(ref IInvoke invoke, ref InvokeEntity invokeProperties)
		{
			base.Build(ref invoke, ref invokeProperties);

			invoke = new DefaultInvokeEvaluator(invokeProperties);
		}

		protected override void Build(ref IContentBody contentBody, ref ContentBody contentBodyProperties)
		{
			base.Build(ref contentBody, ref contentBodyProperties);

			contentBody = new DefaultContentBodyEvaluator(contentBodyProperties);
		}

		protected override void Build(ref IInlineContent inlineContent, ref InlineContent inlineContentProperties)
		{
			base.Build(ref inlineContent, ref inlineContentProperties);

			inlineContent = new DefaultInlineContentEvaluator(inlineContentProperties);
		}
	}
}