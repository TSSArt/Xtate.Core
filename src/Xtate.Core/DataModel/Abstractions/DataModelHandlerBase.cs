#region Copyright © 2019-2021 Sergii Artemenko

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
using Xtate.Core;

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

		public virtual string ConvertToText(DataModelValue value) => value.ToString(provider: null);

		void IDataModelHandler.Process(ref IExecutableEntity executableEntity) => Visit(ref executableEntity);

		void IDataModelHandler.Process(ref IDataModel dataModel) => Visit(ref dataModel);

		void IDataModelHandler.Process(ref IDoneData doneData) => Visit(ref doneData);

		void IDataModelHandler.Process(ref IInvoke invoke) => Visit(ref invoke);

		public virtual bool CaseInsensitive => false;

		public abstract ITypeInfo TypeInfo { get; }

	#endregion

		protected void AddErrorMessage(object? entity, string message, Exception? exception = default) => _errorProcessor.AddError(GetType(), entity, message, exception);

		protected override void Visit(ref ILog log)
		{
			base.Visit(ref log);

			log = new DefaultLogEvaluator(log);
		}

		protected override void Visit(ref ISend send)
		{
			base.Visit(ref send);

			send = new DefaultSendEvaluator(send);
		}

		protected override void Visit(ref IDoneData doneData)
		{
			base.Visit(ref doneData);

			doneData = new DefaultDoneDataEvaluator(doneData);
		}

		protected override void Visit(ref IParam param)
		{
			base.Visit(ref param);

			param = new DefaultParam(param);
		}

		protected override void Visit(ref ICancel cancel)
		{
			base.Visit(ref cancel);

			cancel = new DefaultCancelEvaluator(cancel);
		}

		protected override void Visit(ref IIf @if)
		{
			base.Visit(ref @if);

			@if = new DefaultIfEvaluator(@if);
		}

		protected override void Visit(ref IRaise raise)
		{
			base.Visit(ref raise);

			raise = new DefaultRaiseEvaluator(raise);
		}

		protected override void Visit(ref IForEach forEach)
		{
			base.Visit(ref forEach);

			forEach = new DefaultForEachEvaluator(forEach);
		}

		protected override void Visit(ref IAssign assign)
		{
			base.Visit(ref assign);

			assign = new DefaultAssignEvaluator(assign);
		}

		protected override void Visit(ref IScript script)
		{
			base.Visit(ref script);

			script = new DefaultScriptEvaluator(script);
		}

		protected override void Visit(ref ICustomAction customAction)
		{
			base.Visit(ref customAction);

			customAction = new DefaultCustomActionEvaluator(customAction);
		}

		protected override void Visit(ref IInvoke invoke)
		{
			base.Visit(ref invoke);

			invoke = new DefaultInvokeEvaluator(invoke);
		}

		protected override void Visit(ref IContentBody contentBody)
		{
			base.Visit(ref contentBody);

			contentBody = new DefaultContentBodyEvaluator(contentBody);
		}

		protected override void Visit(ref IInlineContent inlineContent)
		{
			base.Visit(ref inlineContent);

			inlineContent = new DefaultInlineContentEvaluator(inlineContent);
		}

		protected override void Visit(ref IExternalDataExpression externalDataExpression)
		{
			base.Visit(ref externalDataExpression);

			externalDataExpression = new DefaultExternalDataExpressionEvaluator(externalDataExpression);
		}
	}
}