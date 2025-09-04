// Copyright © 2019-2025 Sergii Artemenko
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

using Xtate.CustomAction;

namespace Xtate.DataModel;

public abstract class DataModelHandlerBase : StateMachineVisitor, IDataModelHandler
{
    public required Func<ILog, DefaultLogEvaluator> DefaultLogEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<ISend, DefaultSendEvaluator> DefaultSendEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<ICancel, DefaultCancelEvaluator> DefaultCancelEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IIf, DefaultIfEvaluator> DefaultIfEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IRaise, DefaultRaiseEvaluator> DefaultRaiseEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IForEach, DefaultForEachEvaluator> DefaultForEachEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IAssign, DefaultAssignEvaluator> DefaultAssignEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IScript, DefaultScriptEvaluator> DefaultScriptEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<ICustomAction, DefaultCustomActionEvaluator> DefaultCustomActionEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IContentBody, DefaultContentBodyEvaluator> DefaultContentBodyEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IInlineContent, DefaultInlineContentEvaluator> DefaultInlineContentEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<IExternalDataExpression, DefaultExternalDataExpressionEvaluator> DefaultExternalDataExpressionEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<ICustomAction, CustomActionContainer> CustomActionContainerFactory { private get; [UsedImplicitly] init; }

#region Interface IDataModelHandler

    public virtual string ConvertToText(DataModelValue value) => value.ToString(provider: null);

    void IDataModelHandler.Process(ref IExecutableEntity executableEntity) => Visit(ref executableEntity);

    void IDataModelHandler.Process(ref IValueExpression valueExpression) => Visit(ref valueExpression);

    void IDataModelHandler.Process(ref ILocationExpression locationExpression) => Visit(ref locationExpression);

    void IDataModelHandler.Process(ref IConditionExpression conditionExpression) => Visit(ref conditionExpression);

    void IDataModelHandler.Process(ref IContentBody contentBody) => Visit(ref contentBody);

    void IDataModelHandler.Process(ref IInlineContent inlineContent) => Visit(ref inlineContent);

    void IDataModelHandler.Process(ref IExternalDataExpression externalDataExpression) => Visit(ref externalDataExpression);

    public virtual bool CaseInsensitive => false;

    public virtual ImmutableDictionary<string, string> DataModelVars => ImmutableDictionary<string, string>.Empty;

#endregion

    protected override void Visit(ref ILog log)
    {
        base.Visit(ref log);

        log = GetEvaluator(log);
    }

    protected virtual ILog GetEvaluator(ILog log) => DefaultLogEvaluatorFactory(log);

    protected override void Visit(ref ISend send)
    {
        base.Visit(ref send);

        send = GetEvaluator(send);
    }

    protected virtual ISend GetEvaluator(ISend send) => DefaultSendEvaluatorFactory(send);

    protected override void Visit(ref ICancel cancel)
    {
        base.Visit(ref cancel);

        cancel = GetEvaluator(cancel);
    }

    protected virtual ICancel GetEvaluator(ICancel cancel) => DefaultCancelEvaluatorFactory(cancel);

    protected override void Visit(ref IIf @if)
    {
        base.Visit(ref @if);

        @if = GetEvaluator(@if);
    }

    protected virtual IIf GetEvaluator(IIf @if) => DefaultIfEvaluatorFactory(@if);

    protected override void Visit(ref IRaise raise)
    {
        base.Visit(ref raise);

        raise = GetEvaluator(raise);
    }

    protected virtual IRaise GetEvaluator(IRaise raise) => DefaultRaiseEvaluatorFactory(raise);

    protected override void Visit(ref IForEach forEach)
    {
        base.Visit(ref forEach);

        forEach = GetEvaluator(forEach);
    }

    protected virtual IForEach GetEvaluator(IForEach forEach) => DefaultForEachEvaluatorFactory(forEach);

    protected override void Visit(ref IAssign assign)
    {
        base.Visit(ref assign);

        assign = GetEvaluator(assign);
    }

    protected virtual IAssign GetEvaluator(IAssign assign) => DefaultAssignEvaluatorFactory(assign);

    protected override void Visit(ref IScript script)
    {
        base.Visit(ref script);

        script = GetEvaluator(script);
    }

    protected virtual IScript GetEvaluator(IScript script) => DefaultScriptEvaluatorFactory(script);

    protected override void Visit(ref ICustomAction customAction)
    {
        customAction = CreateCustomActionContainer(customAction);

        base.Visit(ref customAction);

        customAction = GetEvaluator(customAction);
    }

    protected virtual ICustomAction CreateCustomActionContainer(ICustomAction customAction) => CustomActionContainerFactory(customAction);

    protected virtual ICustomAction GetEvaluator(ICustomAction customAction) => DefaultCustomActionEvaluatorFactory(customAction);

    protected override void Visit(ref IContentBody contentBody)
    {
        base.Visit(ref contentBody);

        contentBody = GetEvaluator(contentBody);
    }

    protected virtual IContentBody GetEvaluator(IContentBody contentBody) => DefaultContentBodyEvaluatorFactory(contentBody);

    protected override void Visit(ref IInlineContent inlineContent)
    {
        base.Visit(ref inlineContent);

        inlineContent = GetEvaluator(inlineContent);
    }

    protected virtual IInlineContent GetEvaluator(IInlineContent inlineContent) => DefaultInlineContentEvaluatorFactory(inlineContent);

    protected override void Visit(ref IExternalDataExpression externalDataExpression)
    {
        base.Visit(ref externalDataExpression);

        externalDataExpression = GetEvaluator(externalDataExpression);
    }

    protected virtual IExternalDataExpression GetEvaluator(IExternalDataExpression externalDataExpression) => DefaultExternalDataExpressionEvaluatorFactory(externalDataExpression);
}