#region Copyright © 2019-2022 Sergii Artemenko

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

namespace Xtate.DataModel;

[PublicAPI]
public class DataModelHandlerBaseEvaluatorFactory : IDataModelHandlerBaseEvaluatorFactory
{
	private readonly Func<IAssign, DefaultAssignEvaluator>                                 _assignEvaluatorFactory;
	private readonly Func<ICancel, DefaultCancelEvaluator>                                 _cancelEvaluatorFactory;
	private readonly Func<IContentBody, DefaultContentBodyEvaluator>                       _contentBodyEvaluatorFactory;
	private readonly Func<ICustomAction, DefaultCustomActionEvaluator>                     _customActionEvaluatorFactory;
	private readonly Func<IDoneData, DefaultDoneDataEvaluator>                             _doneDataEvaluatorFactory;
	private readonly Func<IExternalDataExpression, DefaultExternalDataExpressionEvaluator> _externalDataExpressionEvaluatorFactory;
	private readonly Func<IForEach, DefaultForEachEvaluator>                               _forEachEvaluatorFactory;
	private readonly Func<IIf, DefaultIfEvaluator>                                         _ifEvaluatorFactory;
	private readonly Func<IInlineContent, DefaultInlineContentEvaluator>                   _inlineContentEvaluatorFactory;
	private readonly Func<IInvoke, DefaultInvokeEvaluator>                                 _invokeEvaluatorFactory;
	private readonly Func<ILog, DefaultLogEvaluator>                                       _logEvaluatorFactory;
	private readonly Func<IParam, DefaultParamEvaluator>                                            _paramFactory;
	private readonly Func<IRaise, DefaultRaiseEvaluator>                                   _raiseEvaluatorFactory;
	private readonly Func<IScript, DefaultScriptEvaluator>                                 _scriptEvaluatorFactory;
	private readonly Func<ISend, DefaultSendEvaluator>                                     _sendEvaluatorFactory;

	public DataModelHandlerBaseEvaluatorFactory(Func<ILog, DefaultLogEvaluator> logEvaluatorFactory,
											Func<ISend, DefaultSendEvaluator> sendEvaluatorFactory,
											Func<IDoneData, DefaultDoneDataEvaluator> doneDataEvaluatorFactory,
											Func<IParam, DefaultParamEvaluator> paramFactory,
											Func<ICancel, DefaultCancelEvaluator> cancelEvaluatorFactory,
											Func<IIf, DefaultIfEvaluator> ifEvaluatorFactory,
											Func<IRaise, DefaultRaiseEvaluator> raiseEvaluatorFactory,
											Func<IForEach, DefaultForEachEvaluator> forEachEvaluatorFactory,
											Func<IAssign, DefaultAssignEvaluator> assignEvaluatorFactory,
											Func<IScript, DefaultScriptEvaluator> scriptEvaluatorFactory,
											Func<ICustomAction, DefaultCustomActionEvaluator> customActionEvaluatorFactory,
											Func<IInvoke, DefaultInvokeEvaluator> invokeEvaluatorFactory,
											Func<IContentBody, DefaultContentBodyEvaluator> contentBodyEvaluatorFactory,
											Func<IInlineContent, DefaultInlineContentEvaluator> inlineContentEvaluatorFactory,
											Func<IExternalDataExpression, DefaultExternalDataExpressionEvaluator> externalDataExpressionEvaluatorFactory)
	{
		_logEvaluatorFactory = logEvaluatorFactory;
		_sendEvaluatorFactory = sendEvaluatorFactory;
		_doneDataEvaluatorFactory = doneDataEvaluatorFactory;
		_paramFactory = paramFactory;
		_cancelEvaluatorFactory = cancelEvaluatorFactory;
		_ifEvaluatorFactory = ifEvaluatorFactory;
		_raiseEvaluatorFactory = raiseEvaluatorFactory;
		_forEachEvaluatorFactory = forEachEvaluatorFactory;
		_assignEvaluatorFactory = assignEvaluatorFactory;
		_scriptEvaluatorFactory = scriptEvaluatorFactory;
		_customActionEvaluatorFactory = customActionEvaluatorFactory;
		_invokeEvaluatorFactory = invokeEvaluatorFactory;
		_contentBodyEvaluatorFactory = contentBodyEvaluatorFactory;
		_inlineContentEvaluatorFactory = inlineContentEvaluatorFactory;
		_externalDataExpressionEvaluatorFactory = externalDataExpressionEvaluatorFactory;
	}

#region Interface IDataModelHandlerBaseEvaluatorFactory

	public virtual ILog CreateLogEvaluator(ILog log) => _logEvaluatorFactory(log);

	public virtual ISend CreateSendEvaluator(ISend send) => _sendEvaluatorFactory(send);

	public virtual IDoneData CreateDoneDataEvaluator(IDoneData doneData) => _doneDataEvaluatorFactory(doneData);

	public virtual IParam CreateParam(IParam param) => _paramFactory(param);

	public virtual ICancel CreateCancelEvaluator(ICancel cancel) => _cancelEvaluatorFactory(cancel);

	public virtual IIf CreateIfEvaluator(IIf iif) => _ifEvaluatorFactory(iif);

	public virtual IRaise CreateRaiseEvaluator(IRaise raise) => _raiseEvaluatorFactory(raise);

	public virtual IForEach CreateForEachEvaluator(IForEach forEach) => _forEachEvaluatorFactory(forEach);

	public virtual IAssign CreateAssignEvaluator(IAssign assign) => _assignEvaluatorFactory(assign);

	public virtual IScript CreateScriptEvaluator(IScript script) => _scriptEvaluatorFactory(script);

	public virtual ICustomAction CreateCustomActionEvaluator(ICustomAction customAction) => _customActionEvaluatorFactory(customAction);

	public virtual IInvoke CreateInvokeEvaluator(IInvoke invoke) => _invokeEvaluatorFactory(invoke);

	public virtual IContentBody CreateContentBodyEvaluator(IContentBody contentBody) => _contentBodyEvaluatorFactory(contentBody);

	public virtual IInlineContent CreateInlineContentEvaluator(IInlineContent inlineContent) => _inlineContentEvaluatorFactory(inlineContent);

	public virtual IExternalDataExpression CreateExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) => _externalDataExpressionEvaluatorFactory(externalDataExpression);

#endregion
}