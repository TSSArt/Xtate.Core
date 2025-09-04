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

namespace Xtate.DataModel.Runtime;

public class RuntimeDataModelHandler : DataModelHandlerBase
{
    public class Provider() : DataModelHandlerProviderBase<RuntimeDataModelHandler>(@"runtime");

    public required Func<RuntimePredicate, RuntimePredicateEvaluator> RuntimePredicateEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<RuntimeValue, RuntimeValueEvaluator> RuntimeValueEvaluatorFactory { private get; [UsedImplicitly] init; }

    public required Func<RuntimeAction, RuntimeActionExecutor> RuntimeActionExecutorFactory { private get; [UsedImplicitly] init; }

    public required IErrorProcessorService<RuntimeDataModelHandler> RuntimeErrorProcessorService { private get; [UsedImplicitly] init; }

    protected override void Visit(ref IScript script) => RuntimeErrorProcessorService.AddError(script, Resources.ErrorMessage_ScriptingNotSupportedInRuntimeDataModel);

    protected override void Visit(ref IDataModel dataModel) => RuntimeErrorProcessorService.AddError(dataModel, Resources.ErrorMessage_DataModelNotSupportedInRuntime);

    protected override void Visit(ref IConditionExpression conditionExpression)
    {
        if (conditionExpression is RuntimePredicate runtimePredicate)
        {
            conditionExpression = RuntimePredicateEvaluatorFactory(runtimePredicate);
        }
        else
        {
            RuntimeErrorProcessorService.AddError(conditionExpression, Resources.ErrorMessage_RuntimePredicateOnlyAllowed);
        }
    }

    protected override void Visit(ref IValueExpression valueExpression)
    {
        if (valueExpression is RuntimeValue runtimeValue)
        {
            valueExpression = RuntimeValueEvaluatorFactory(runtimeValue);
        }
        else
        {
            RuntimeErrorProcessorService.AddError(valueExpression, Resources.ErrorMessage_RuntimeValueOnlyAllowed);
        }
    }

    protected override void VisitUnknown(ref IExecutableEntity executableEntity)
    {
        if (executableEntity is RuntimeAction runtimeAction)
        {
            executableEntity = RuntimeActionExecutorFactory(runtimeAction);
        }
        else
        {
            RuntimeErrorProcessorService.AddError(executableEntity, Resources.ErrorMessage_RuntimeActionOnlyAllowed);
        }
    }
}