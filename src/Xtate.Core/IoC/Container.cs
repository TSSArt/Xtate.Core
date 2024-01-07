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

// ReSharper disable All
using Xtate.Builder;
using Xtate.DataModel;
using Xtate.DataModel.Null;
using Xtate.DataModel.Runtime;
using Xtate.DataModel.XPath;
using Xtate.Scxml;
using Xtate.IoC;
using Xtate.XInclude;

namespace Xtate.Core
{
	//TODO: This class should be deleted when full migration to DI will be completed
#pragma warning disable 1998

	public static class Container
	{
		public static void Setup(ServiceCollection services)
		{
			Infra.Requires(services);

			services.AddShared<ISecurityContext>(SharedWithin.Scope, async _ => SecurityContext.Create(SecurityContextType.NewTrustedStateMachine));
			services.AddForwarding(async sp => (IIoBoundTask) await sp.GetRequiredService<ISecurityContext>().ConfigureAwait(false));
			//services.AddForwarding(sp => new ServiceLocator(sp));
			services.AddImplementation<ResourceLoaderService>().For<IResourceLoader>();
			services.AddSharedImplementation<StateMachineInterpreter>(SharedWithin.Scope).For<IStateMachineInterpreter>();
			services.AddSharedImplementation<StateMachineRunner>(SharedWithin.Scope).For<IStateMachineRunner>();
			services.AddSharedImplementation<ScopeManager>(SharedWithin.Scope).For<IScopeManager>();
			services.AddType<FileResourceLoader>();
			services.AddType<ResxResourceLoader>();
			services.AddType<WebResourceLoader>();
			services.AddTypeSync<DefaultAssignEvaluator, IAssign>();
			services.AddTypeSync<DefaultCancelEvaluator, ICancel>();
			services.AddTypeSync<DefaultContentBodyEvaluator, IContentBody>();
			services.AddTypeSync<DefaultCustomActionEvaluator, ICustomAction>();
			services.AddTypeSync<DefaultExternalDataExpressionEvaluator, IExternalDataExpression>();
			services.AddTypeSync<DefaultForEachEvaluator, IForEach>();
			services.AddTypeSync<DefaultIfEvaluator, IIf>();
			services.AddTypeSync<DefaultInlineContentEvaluator, IInlineContent>();
			services.AddTypeSync<DefaultLogEvaluator, ILog>();
			services.AddTypeSync<DefaultRaiseEvaluator, IRaise>();
			services.AddTypeSync<DefaultScriptEvaluator, IScript>();
			services.AddTypeSync<DefaultSendEvaluator, ISend>();
			services.AddType<NullDataModelHandler>();
			services.AddType<RuntimeDataModelHandler>();
			services.AddType<XPathDataModelHandler>();

			//services.AddImplementation<ErrorProcessorService<Any>>().For<IErrorProcessorService<Any>>();
			services.AddImplementation<DataModelHandlerService>().For<IDataModelHandlerService>();
			services.AddSharedImplementationSync<DefaultErrorProcessor>(SharedWithin.Container).For<IErrorProcessor>();
			//services.AddImplementation<BuilderFactory>().For<IBuilderFactory>();
			services.AddImplementation<StateMachineValidator>()
					.For<IStateMachineValidator>()
					.For<StateMachineValidator>(); //TODO: remove .For<StateMachineValidator>()
			services.AddImplementation<EventQueue>().For<IEventQueueReader>();
			services.AddFactory<DataModelHandlerGetter>().For<IDataModelHandler>();

			//services.AddImplementation<TraceLogger>().For<ILoggerOld>();
			services.AddSharedImplementation<NullDataModelHandlerProvider>(SharedWithin.Container).For<IDataModelHandlerProvider>();
			services.AddSharedImplementation<RuntimeDataModelHandlerProvider>(SharedWithin.Container).For<IDataModelHandlerProvider>();
			services.AddSharedImplementation<XPathDataModelHandlerProvider>(SharedWithin.Container).For<IDataModelHandlerProvider>();
			services.AddSharedImplementation<FileResourceLoaderProvider>(SharedWithin.Container).For<IResourceLoaderProvider>();
			services.AddSharedImplementation<ResxResourceLoaderProvider>(SharedWithin.Container).For<IResourceLoaderProvider>();
			services.AddSharedImplementation<WebResourceLoaderProvider>(SharedWithin.Container).For<IResourceLoaderProvider>();
			services.AddSharedImplementation<StateMachineRuntimeController>(SharedWithin.Scope).For<IStateMachineController>();

			//.For<StateMachineRuntimeController>(); //TODO: remove
			services.AddImplementation<XIncludeOptions>().For<IXIncludeOptions>();

			//services.AddSharedImplementation<StateMachineControllerProxy>(SharedWithin.Scope).For<IStateMachineController>();
			services.AddImplementation<StateMachineInterpreterOptions>().For<IStateMachineInterpreterOptions>();
			services.AddSharedImplementation<InterpreterModel>(SharedWithin.Scope).For<IInterpreterModel>();
			services.AddType<InterpreterModelBuilder>();

			//services.AddImplementation<PreDataModelProcessor>().For<IPreDataModelProcessor>();
			services.AddImplementation<StateMachineSessionId>().For<IStateMachineSessionId>();

			if (!services.IsRegistered<IStateMachine>())
			{
				services.AddFactory<StateMachineGetter>().For<IStateMachine>();
				services.AddImplementation<StateMachineService>().For<IStateMachineService>();
				services.AddImplementation<ScxmlStateMachineProvider>().For<IStateMachineProvider>();
				services.AddImplementation<SourceStateMachineProvider>().For<IStateMachineProvider>();
				services.AddType<ScxmlReaderStateMachineGetter>();
				services.AddType<ScxmlLocationStateMachineGetter>();
				services.AddImplementation<RedirectXmlResolver>().For<ScxmlXmlResolver>();
			}

			services.AddType<ScxmlDirector>();

			//services.AddSharedImplementation<StateMachineContextOptions>(SharedWithin.Scope).For<IStateMachineContextOptions>();
			//services.AddSharedImplementation<ExecutionContextOptions>(SharedWithin.Scope).For<IExecutionContextOptions>();
			services.AddSharedImplementation<StateMachineContext>(SharedWithin.Scope).For<IStateMachineContext>();
			services.AddImplementation<StateMachineStartOptions>().For<IStateMachineStartOptions>();

			services.AddTypeSync<StateMachineFluentBuilder>();
			services.AddTypeSync<StateFluentBuilder<Any>, Any, Action<IState>>();
			services.AddTypeSync<ParallelFluentBuilder<Any>, Any, Action<IParallel>>();
			services.AddTypeSync<FinalFluentBuilder<Any>, Any, Action<IFinal>>();
			services.AddTypeSync<InitialFluentBuilder<Any>, Any, Action<IInitial>>();
			services.AddTypeSync<HistoryFluentBuilder<Any>, Any, Action<IHistory>>();
			services.AddTypeSync<TransitionFluentBuilder<Any>, Any, Action<ITransition>>();

			services.AddImplementationSync<ErrorProcessorService<Any>>().For<IErrorProcessorService<Any>>();

			services.AddImplementationSync<StateMachineBuilder>().For<IStateMachineBuilder>();
			services.AddImplementationSync<StateBuilder>().For<IStateBuilder>();
			services.AddImplementationSync<ParallelBuilder>().For<IParallelBuilder>();
			services.AddImplementationSync<HistoryBuilder>().For<IHistoryBuilder>();
			services.AddImplementationSync<InitialBuilder>().For<IInitialBuilder>();
			services.AddImplementationSync<FinalBuilder>().For<IFinalBuilder>();
			services.AddImplementationSync<TransitionBuilder>().For<ITransitionBuilder>();
			services.AddImplementationSync<LogBuilder>().For<ILogBuilder>();
			services.AddImplementationSync<SendBuilder>().For<ISendBuilder>();
			services.AddImplementationSync<ParamBuilder>().For<IParamBuilder>();

			services.AddImplementationSync<ContentBuilder>().For<IContentBuilder>();
			services.AddImplementationSync<OnEntryBuilder>().For<IOnEntryBuilder>();
			services.AddImplementationSync<OnExitBuilder>().For<IOnExitBuilder>();
			services.AddImplementationSync<InvokeBuilder>().For<IInvokeBuilder>();
			services.AddImplementationSync<FinalizeBuilder>().For<IFinalizeBuilder>();
			services.AddImplementationSync<ScriptBuilder>().For<IScriptBuilder>();
			services.AddImplementationSync<DataModelBuilder>().For<IDataModelBuilder>();
			services.AddImplementationSync<DataBuilder>().For<IDataBuilder>();
			services.AddImplementationSync<DoneDataBuilder>().For<IDoneDataBuilder>();
			services.AddImplementationSync<ForEachBuilder>().For<IForEachBuilder>();
			services.AddImplementationSync<IfBuilder>().For<IIfBuilder>();
			services.AddImplementationSync<ElseBuilder>().For<IElseBuilder>();
			services.AddImplementationSync<ElseIfBuilder>().For<IElseIfBuilder>();
			services.AddImplementationSync<RaiseBuilder>().For<IRaiseBuilder>();
			services.AddImplementationSync<AssignBuilder>().For<IAssignBuilder>();
			services.AddImplementationSync<CancelBuilder>().For<ICancelBuilder>();
			services.AddImplementationSync<CustomActionBuilder>().For<ICustomActionBuilder>();

			services.AddTypeSync<DefaultAssignEvaluator, IAssign>();
			services.AddTypeSync<DefaultCancelEvaluator, ICancel>();
			services.AddTypeSync<DefaultContentBodyEvaluator, IContentBody>();
			services.AddTypeSync<DefaultCustomActionEvaluator, ICustomAction>();
			services.AddTypeSync<DefaultExternalDataExpressionEvaluator, IExternalDataExpression>();
			services.AddTypeSync<DefaultForEachEvaluator, IForEach>();
			services.AddTypeSync<DefaultIfEvaluator, IIf>();
			services.AddTypeSync<DefaultInlineContentEvaluator, IInlineContent>();
			services.AddTypeSync<DefaultLogEvaluator, ILog>();
			services.AddTypeSync<DefaultRaiseEvaluator, IRaise>();
			services.AddTypeSync<DefaultScriptEvaluator, IScript>();
			services.AddTypeSync<DefaultSendEvaluator, ISend>();

			services.AddType<UnknownDataModelHandler>();
			services.AddType<NullDataModelHandler>();
			services.AddType<RuntimeDataModelHandler>();

			services.AddImplementation<NullDataModelHandlerProvider>().For<IDataModelHandlerProvider>();
			services.AddImplementation<RuntimeDataModelHandlerProvider>().For<IDataModelHandlerProvider>();

			//services.AddXPathDataModelHandler();

			services.AddImplementation<DataModelHandlerService>().For<IDataModelHandlerService>();
			services.AddFactory<DataModelHandlerGetter>().For<IDataModelHandler>();

			services.AddTypeSync<RuntimeActionExecutor, RuntimeAction>();
			services.AddTypeSync<RuntimeValueEvaluator, RuntimeValue>();
			services.AddTypeSync<RuntimePredicateEvaluator, RuntimePredicate>();

			//services.AddType<RuntimeEvaluatorFunc, Evaluator>();
			//services.AddType<RuntimeEvaluatorTask, EvaluatorTask>();
			//services.AddType<RuntimeEvaluatorCancellableTask, EvaluatorCancellableTask>();
		}
	}
}