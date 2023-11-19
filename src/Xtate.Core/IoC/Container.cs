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

// ReSharper disable All
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Builder;
using Xtate.Core;
using Xtate.DataModel;
using Xtate.DataModel.Null;
using Xtate.DataModel.Runtime;
using Xtate.DataModel.XPath;
using Xtate.Scxml;
using Xtate.IoC;
using Xtate.XInclude;
using IServiceProvider = Xtate.IoC.IServiceProvider;

namespace Xtate
{
	//TODO: Replace ServiceLocator to DI
	/// <summary>
	///     temporary class
	/// </summary>
	public readonly struct ServiceLocator
	{
		public static readonly ServiceLocator Default;

		private readonly IServiceProvider _serviceProvider;

		static ServiceLocator()
		{
			var services = new ServiceCollection();

			Container.Setup(services);

			Default = new ServiceLocator(services.BuildProvider());
		}

		public ServiceLocator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

		public static ServiceLocator Create(Action<IServiceCollection> setup)
		{
			if (setup is null) throw new ArgumentNullException(nameof(setup));

			var services = new ServiceCollection();

			Container.Setup(services);

			setup(services);

			return new ServiceLocator(services.BuildProvider());
		}

		public T GetService<T>() => _serviceProvider.GetRequiredService<T>().SynchronousGetResult();

		public Func<TArg, T> GetFactory<T, TArg>() => _serviceProvider.GetRequiredSyncFactory<T, TArg>();

		public T GetService<T, TArg>(TArg arg) => _serviceProvider.GetRequiredService<T, TArg>(arg).SynchronousGetResult();

		public T? GetOptionalService<T>() => _serviceProvider.GetOptionalService<T>().SynchronousGetResult();

		public IAsyncEnumerable<T> GetServices<T>() => _serviceProvider.GetServices<T>();
	}
}

namespace Xtate.Core
{
	//TODO: This class should be deleted when full migration to DI will be completed
	#pragma warning disable 1998

	public static class ContainerExtensions
	{
		public static void AddForwarding<TFrom, TTo>(this IServiceCollection services) where TFrom : class where TTo : class, TFrom
		{
			if (services is null) throw new ArgumentNullException(nameof(services));

			services.AddForwarding<TFrom?>(static async serviceProvider => await serviceProvider.GetOptionalService<TTo>().ConfigureAwait(false));
		}

		public static void AddForwarding<TFrom, TTo, TArg>(this IServiceCollection services) where TFrom : class where TTo : class, TFrom
		{
			if (services is null) throw new ArgumentNullException(nameof(services));

			services.AddForwarding<TFrom?, TArg>(static async (serviceProvider, argument) => await serviceProvider.GetOptionalService<TTo, TArg>(argument).ConfigureAwait(false));
		}
	}

	public static class Container
	{
		public static void Setup(ServiceCollection services)
		{
			Infra.Requires(services);

			services.AddShared<ISecurityContext>(SharedWithin.Scope, async _ => SecurityContext.Create(SecurityContextType.NewTrustedStateMachine));
			services.AddForwarding(async sp => (IIoBoundTask) await sp.GetRequiredService<ISecurityContext>().ConfigureAwait(false));
			services.AddForwarding(sp => new ServiceLocator(sp));
			services.AddImplementation<ResourceLoaderService>().For<IResourceLoader>();
			services.AddSharedImplementation<StateMachineInterpreter>(SharedWithin.Scope).For<IStateMachineInterpreter>();
			services.AddSharedImplementation<StateMachineRunner>(SharedWithin.Scope).For<IStateMachineRunner>();
			services.AddSharedImplementation<ScopeManager>(SharedWithin.Scope).For<IScopeManager>();
			services.AddType<FileResourceLoader>();
			services.AddType<ResxResourceLoader>();
			services.AddType<WebResourceLoader>();
			services.AddImplementation<DataModelHandlerBaseEvaluatorFactory>().For<IDataModelHandlerBaseEvaluatorFactory>();
			services.AddTypeSync<DefaultAssignEvaluator, IAssign>();
			services.AddTypeSync<DefaultCancelEvaluator, ICancel>();
			services.AddTypeSync<DefaultContentBodyEvaluator, IContentBody>();
			services.AddTypeSync<DefaultCustomActionEvaluator, ICustomAction>();
			services.AddTypeSync<DefaultDoneDataEvaluator, IDoneData>();
			services.AddTypeSync<DefaultExternalDataExpressionEvaluator, IExternalDataExpression>();
			services.AddTypeSync<DefaultForEachEvaluator, IForEach>();
			services.AddTypeSync<DefaultIfEvaluator, IIf>();
			services.AddTypeSync<DefaultInlineContentEvaluator, IInlineContent>();
			services.AddTypeSync<DefaultInvokeEvaluator, IInvoke>();
			services.AddTypeSync<DefaultLogEvaluator, ILog>();
			services.AddTypeSync<DefaultParamEvaluator, IParam>();
			services.AddTypeSync<DefaultRaiseEvaluator, IRaise>();
			services.AddTypeSync<DefaultScriptEvaluator, IScript>();
			services.AddTypeSync<DefaultSendEvaluator, ISend>();
			services.AddType<NullDataModelHandler>();
			services.AddType<RuntimeDataModelHandler>();
			services.AddType<XPathDataModelHandler>();
			//services.AddImplementation<ErrorProcessorService<Any>>().For<IErrorProcessorService<Any>>();
			services.AddImplementation<DataModelHandlerService>().For<IDataModelHandlerService>();
			services.AddSharedImplementationSync<DefaultErrorProcessor>(SharedWithin.Container).For<IErrorProcessor>();
			services.AddImplementation<BuilderFactory>().For<IBuilderFactory>();
			services.AddImplementation<StateMachineValidator>().For<IStateMachineValidator>()
					.For<StateMachineValidator>(); //TODO: remove .For<StateMachineValidator>()
			services.AddImplementation<EventQueue>().For<IEventQueueReader>();
			services.AddFactory<DataModelHandlerGetter>().For<IDataModelHandler>();
			services.AddImplementation<TraceLogger>().For<ILoggerOld>();
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
			services.AddImplementation<PreDataModelProcessor>().For<IPreDataModelProcessor>();
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
			services.AddSharedImplementation<StateMachineContextOptions>(SharedWithin.Scope).For<IStateMachineContextOptions>();
			services.AddSharedImplementation<ExecutionContextOptions>(SharedWithin.Scope).For<IExecutionContextOptions>();
			services.AddSharedImplementation<StateMachineContext>(SharedWithin.Scope).For<IStateMachineContext>();
			services.AddSharedImplementation<ExecutionContext>(SharedWithin.Scope).For<IExecutionContext>();
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
			services.AddTypeSync<DefaultDoneDataEvaluator, IDoneData>();
			services.AddTypeSync<DefaultExternalDataExpressionEvaluator, IExternalDataExpression>();
			services.AddTypeSync<DefaultForEachEvaluator, IForEach>();
			services.AddTypeSync<DefaultIfEvaluator, IIf>();
			services.AddTypeSync<DefaultInlineContentEvaluator, IInlineContent>();
			services.AddTypeSync<DefaultInvokeEvaluator, IInvoke>();
			services.AddTypeSync<DefaultLogEvaluator, ILog>();
			services.AddTypeSync<DefaultParamEvaluator, IParam>();
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

	public class XIncludeOptions : IXIncludeOptions
	{
	#region Interface IXIncludeOptions

		public bool               XIncludeAllowed   => true;
		public int                MaxNestingLevel   => 10;

	#endregion
	}

	public interface IScopeManager
	{
		ValueTask<IStateMachineController> RunStateMachine(IStateMachineStartOptions stateMachineStartOptions);
	}

	public interface IStateMachineRunner
	{
		ValueTask<IStateMachineController> Run(CancellationToken token);

		ValueTask Wait(CancellationToken token);
	}

	public class HostBasedSourceStateMachine : ISourceStateMachine
	{
		private readonly IHostBaseUri?       _hostBaseUri;
		private readonly ISourceStateMachine _sourceStateMachine;

		public HostBasedSourceStateMachine(ISourceStateMachine sourceStateMachine, IHostBaseUri? hostBaseUri)
		{
			_sourceStateMachine = sourceStateMachine;
			_hostBaseUri = hostBaseUri;
		}

	#region Interface ISourceStateMachine

		public Uri Location => _hostBaseUri?.HostBaseUri.CombineWith(_sourceStateMachine.Location)!;

	#endregion
	}

	public class StateMachineGetter : IAsyncInitialization
	{
		private readonly CancellationTokenSource   _cancellationTokenSource = new();
		private readonly IHostBaseUri?             _hostBaseUri;
		private readonly IScxmlStateMachine?       _scxmlStateMachine;
		private readonly ServiceLocator            _serviceLocator;
		private readonly ISourceStateMachine?      _sourceStateMachine;
		//public  required IStateMachineService      _stateMachineService;
		private readonly IStateMachineStartOptions _stateMachineStartOptions;
		private readonly AsyncInit<IStateMachine>  _stateMachineAsyncInit;

		public Task Initialization => _stateMachineAsyncInit.Task;

		public StateMachineGetter(IStateMachineService stateMachineService)
		{
			//_stateMachineService = stateMachineService;

			_stateMachineAsyncInit = AsyncInit.RunNow(stateMachineService, svc => svc.GetStateMachine());
		}

		//TODO:delete
		[Obsolete]
		public StateMachineGetter(IScxmlStateMachine? scxmlStateMachine, ISourceStateMachine? sourceStateMachine, ServiceLocator serviceLocator)
		{
			//_stateMachineStartOptions = stateMachineStartOptions;
			_scxmlStateMachine = scxmlStateMachine;
			_sourceStateMachine = sourceStateMachine;
			_serviceLocator = serviceLocator;

			//		_hostBaseUri = hostBaseUri;

			//_stateMachineBase = stateMachineStartOptions.Origin.AsStateMachine();
		}

		public ValueTask<IStateMachine> GetStateMachine() => new(_stateMachineAsyncInit.Value);

		//TODO:delete
		/*
		public async ValueTask<IStateMachine> GetStateMachine1(CancellationToken token)
		{
			if (_runtimeStateMachine is not null)
			{
				_stateMachineBase = _runtimeStateMachine.RuntimeStateMachine;
			}
			else if(_scxmlStateMachine is not null)
			{
				_stateMachineBase = await GetStateMachine(_scxmlStateMachine.Location, _scxmlStateMachine.ScxmlStateMachine).ConfigureAwait(false);
			}

			//var origin = _stateMachineStartOptions.Origin;

			//var location = _hostBaseUri?.HostBaseUri.CombineWith(origin.BaseUri);

			switch (origin.Type)
			{
				case StateMachineOriginType.StateMachine:
					_stateMachineBase = origin.AsStateMachine();
					break;

				case StateMachineOriginType.Scxml:
					_stateMachineBase = await GetStateMachine(location, origin.AsScxml()).ConfigureAwait(false);
					break;

				case StateMachineOriginType.Source:
				
					location = location.CombineWith(origin.AsSource());
					_stateMachineBase = await GetStateMachine(location, scxml: default).ConfigureAwait(false);
					break;
				
				default:
					throw new ArgumentException(Resources.Exception_StateMachineOriginMissed);
			}
		}
		*/ /*
		private async ValueTask<IStateMachine> CreateStateMachine(IScxmlStateMachine scxmlStateMachine)
		{
			var nameTable = new NameTable();
			var xmlResolver = new RedirectXmlResolver(_serviceLocator, _cancellationTokenSource.Token);
			var xmlParserContext = GetXmlParserContext(nameTable, uri);
			var xmlReaderSettings = GetXmlReaderSettings(nameTable, xmlResolver);
			var directorOptions = GetScxmlDirectorOptions(_serviceLocator, xmlParserContext, xmlReaderSettings, xmlResolver);

			using var xmlReader = scxml is null
				? XmlReader.Create(uri!.ToString(), xmlReaderSettings, xmlParserContext)
				: XmlReader.Create(new StringReader(scxml), xmlReaderSettings, xmlParserContext);

			var scxmlDirector = new ScxmlDirector(xmlReader, GetBuilderFactory(), directorOptions);

			return await scxmlDirector.ConstructStateMachine().ConfigureAwait(false);
		}*/
	}

	public interface IStateMachineProvider
	{
		ValueTask<IStateMachine?> TryGetStateMachine();
	}

	public interface IStateMachineService
	{
		ValueTask<IStateMachine?> GetStateMachine();
	}

	public class StateMachineService : IStateMachineService
	{
		public required IAsyncEnumerable<IStateMachineProvider> StateMachineProviders { private get; init; }
		public required IServiceProvider dd { private get; init; } //TODO:Delete

	#region Interface IStateMachineService

		public async ValueTask<IStateMachine?> GetStateMachine()
		{
			await foreach (var stateMachineProvider in StateMachineProviders.ConfigureAwait(false))
			{
				if (await stateMachineProvider.TryGetStateMachine().ConfigureAwait(false) is { } stateMachine)
				{
					return stateMachine;
				}
			}

			return default;

			//throw new InfrastructureException(Res.Format(Resources.Exception_CantFindStateMachineProvider));
		}

	#endregion
	}

	public class ScxmlStateMachineProvider : IStateMachineProvider
	{
		public required Func<ValueTask<ScxmlReaderStateMachineGetter>> ScxmlReaderStateMachineGetter { private get; init; }
		public required IScxmlStateMachine?                            ScxmlStateMachine             { private get; init; }

	#region Interface IStateMachineProvider

		public async ValueTask<IStateMachine?> TryGetStateMachine() =>
			ScxmlStateMachine is not null
				? await (await ScxmlReaderStateMachineGetter().ConfigureAwait(false)).GetStateMachine().ConfigureAwait(false)
				: default;

	#endregion
	}

	public class SourceStateMachineProvider : IStateMachineProvider
	{
		public required Func<ValueTask<ScxmlLocationStateMachineGetter>> ScxmlLocationStateMachineGetter { private get; init; }
		public required IStateMachineLocation?                           StateMachineLocation            { private get; init; }
		public required IServiceProvider                           sdf { private get; init; } //TODO:delete

		#region Interface IStateMachineProvider

		public async ValueTask<IStateMachine?> TryGetStateMachine() =>
			StateMachineLocation is { Location: { } }
				? await (await ScxmlLocationStateMachineGetter().ConfigureAwait(false)).GetStateMachine().ConfigureAwait(false)
				: default;

	#endregion
	}

	public readonly struct LifetimeController
	{
		private readonly CancellationTokenSource _cancellationTokenSource;

		public LifetimeController()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}
	}

	public static class AsyncInit
	{
		/// <summary>
		/// Runs delegate <paramref name="init"/> immediately. If no asynchronous operations in <paramref name="init"/> after exit object is completly initialized.
		/// Caution: while executing <paramref name="init"/> delegate object cam be partially initialized. Consider method <see cref="RunAfter{T,TArg}(TArg, Func{TArg,ValueTask{T}})"/> to make sure object fully initialized including required properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="init">Initialization action</param>
		/// <returns></returns>
		public static AsyncInit<T> RunNow<T>(Func<ValueTask<T>> init) => new InitNow<T>(init);

		/// <summary>
		/// Runs delegate <paramref name="init"/> immediately. If no asynchronous operations in <paramref name="init"/> after exit object is completly initialized.
		/// Caution: while executing <paramref name="init"/> delegate object cam be partially initialized. Consider method <see cref="RunAfter{T,TArg}(TArg, Func{TArg,ValueTask{T}})"/> to make sure object fully initialized including required properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TArg"></typeparam>
		/// <param name="arg">Argument</param>
		/// <param name="init">Initialization action</param>
		/// <returns></returns>
		public static AsyncInit<T> RunNow<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> init) => new InitNow<T, TArg>(arg, init);
		/*
		/// <summary>
		/// Runs delegate <paramref name="init"/> immediately. If no asynchronous operations in <paramref name="init"/> after exit object is completly initialized.
		/// Caution: while executing <paramref name="init"/> delegate object can be partially initialized. Consider method <see cref="RunAfter{T}(Func{CancellationToken, ValueTask{T}})"/> to make sure object fully initialized including required properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="init">Initialization action</param>
		/// <returns></returns>
		public static AsyncInitDisposable<T> RunNow<T>(Func<CancellationToken, ValueTask<T>> init) => new AsyncInitDisposableNow<T>(init);*/

		/// <summary>
		/// Runs delegate <param name="init">init</param> after completing constructors and seting up required fields and properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TArg"></typeparam>
		/// <param name="arg">Argument</param>
		/// <param name="init">Initialization action</param>
		/// <returns></returns>
		public static AsyncInit<T> RunAfter<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> init) => new InitAfter<T, TArg>(arg, init);
		/*
		/// <summary>
		/// Runs delegate <param name="init">init</param> after completing constructors and seting up required fields and properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="init">Initialization action</param>
		/// <returns></returns>
		public static AsyncInitDisposable<T> RunAfter<T>(Func<CancellationToken, ValueTask<T>> init) => new AsyncInitDisposableAfter<T>(init);*/

		private sealed class InitNow<T> : AsyncInit<T>
		{
			public InitNow(Func<ValueTask<T>> func)
			{
				Infra.Requires(func);

				Task = Init(func);
			}

			public override Task Task { get; }

			private async Task Init(Func<ValueTask<T>> func) => SetValue(await func().ConfigureAwait(false));
		}

		private sealed class InitNow<T, TArg> : AsyncInit<T>
		{
			public InitNow(TArg arg, Func<TArg, ValueTask<T>> func)
			{
				Infra.Requires(func);

				Task = Init(arg, func);
			}

			public override Task Task { get; }

			private async Task Init(TArg arg, Func<TArg, ValueTask<T>> func) => SetValue(await func(arg).ConfigureAwait(false));
		}

		private sealed class InitAfter<T, TArg> : AsyncInit<T>
		{
			private readonly TArg                     _arg;
			private readonly Func<TArg, ValueTask<T>> _func;
			private          Task                     _task;

			public InitAfter(TArg arg, Func<TArg, ValueTask<T>> func)
			{
				_arg = arg;
				_func = func;
			}

			public override Task Task
			{
				get
				{
					if (_task is { } task)
					{
						return task;
					}
				
					lock (this)
					{
						return _task ??= Init();
					}
				}
			}

			private async Task Init() => SetValue(await _func(_arg).ConfigureAwait(false));
		}
	}

	public abstract class AsyncInit<T>
	{
		private T? _value;

		public abstract Task Task { get; }

		public T Value => TryGetValue(out var value) ? value : Infra.Fail<T>(Resources.ErrorMessage_Not_initialized);

		private bool TryGetValue([MaybeNullWhen(false)] out T value)
		{
			if (Task.Status == TaskStatus.RanToCompletion)
			{
				value = _value!;

				return true;
			}

			value = default;

			return false;
		}

		protected void SetValue(T value) => _value = value;
	}

	public class DisposingToken : CancellationTokenSource
	{
		public DisposingToken() => Token = base.Token;

		public new CancellationToken Token { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					if (!IsCancellationRequested)
					{
						Cancel();
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore
				}
			}

			base.Dispose(disposing);
		}
	}

	/*

	public abstract class AsyncInitDisposable<T> : IDisposable
	{
		private CancellationTokenSource? _cancellationTokenSource;

		private T? _value;

		public abstract Task Task { get; }

		public T Value => TryGetValue(out var value) ? value : Infra.Fail<T>(Resources.ErrorMessage_Not_initialized);

		private bool TryGetValue([MaybeNullWhen(false)] out T value)
		{
			if (Task.Status == TaskStatus.RanToCompletion)
			{
				value = _value!;

				return true;
			}

			value = default;

			return false;
		}

	#region Interface IDisposable

		public void Dispose()
		{
			Dispose(true);
			
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _cancellationTokenSource is { } cts)
			{
				try
				{
					cts.Cancel();
				}
				catch (ObjectDisposedException) { }
			}
		}

	#endregion

		protected async Task Init(Func<CancellationToken, ValueTask<T>> func)
		{
			Infra.Requires(func);

			_cancellationTokenSource = new CancellationTokenSource();
			var token = _cancellationTokenSource.Token;

			try
			{
				var value = await func(token).ConfigureAwait(false);

				token.ThrowIfCancellationRequested();

				_value = value;
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == token)
			{
				throw new ObjectDisposedException(Resources.Exception_ObjectDisposedDuringInitialization, ex);
			}
			finally
			{
				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = null;
			}
		}
	}

	public sealed class AsyncInitDisposableNow<T> : AsyncInitDisposable<T>
	{
		public AsyncInitDisposableNow(Func<CancellationToken, ValueTask<T>> func)
		{
			Infra.Requires(func);

			Task = Init(func);
		}

		public override Task Task { get; }
	}

	public sealed class AsyncInitDisposableAfter<T> : AsyncInitDisposable<T>
	{
		private Lazy<Task> _lazyTask;

		public AsyncInitDisposableAfter(Func<CancellationToken, ValueTask<T>> func)
		{
			Infra.Requires(func);

			_lazyTask = new Lazy<Task>(() => Init(func), LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public override Task Task => _lazyTask.Value;
	}*/

	public class ScxmlReaderStateMachineGetter
	{
		//public required Func<XmlReader, ValueTask<ScxmlDirector>> _scxmlDirectorFactory { private get; init; }
		public required IScxmlDeserializer _ScxmlDeserializer { private get; init; }
		public required IScxmlStateMachine                        _scxmlStateMachine    { private get; init; }
		public required ScxmlXmlResolver                          _scxmlXmlResolver     { private get; init; }
		public required IStateMachineLocation?                    _stateMachineLocation { private get; init; }
		public required INameTableProvider?                       NameTableProvider     { private get; init; }
		public required IStateMachineValidator                    StateMachineValidator { private get; init; }

		public async ValueTask<IStateMachine> GetStateMachine()
		{
			using var xmlReader = CreateXmlReader();

			var stateMachine =  await _ScxmlDeserializer.Deserialize(xmlReader).ConfigureAwait(false);

			StateMachineValidator.Validate(stateMachine);

			return stateMachine;
		}

		protected virtual XmlReader CreateXmlReader() => XmlReader.Create(_scxmlStateMachine.CreateTextReader(), GetXmlReaderSettings(), GetXmlParserContext());

		protected virtual XmlReaderSettings GetXmlReaderSettings() =>
			new()
			{
				Async = true,
				CloseInput = true,
				XmlResolver = _scxmlXmlResolver,
				DtdProcessing = DtdProcessing.Parse
			};

		protected virtual XmlParserContext GetXmlParserContext()
		{
			var nameTable = NameTableProvider?.GetNameTable() ?? new NameTable();
			var nsManager = new XmlNamespaceManager(nameTable);

			return new XmlParserContext(nameTable, nsManager, xmlLang: null, XmlSpace.None) { BaseURI = _stateMachineLocation?.Location.ToString() };
		}
	}

	public class ScxmlLocationStateMachineGetter
	{
		public required  IScxmlDeserializer                        _ScxmlDeserializer { private get; init; }
		private readonly Func<XmlReader, ValueTask<ScxmlDirector>> _scxmlDirectorFactory;
		private readonly ScxmlXmlResolver                          _scxmlXmlResolver;
		private readonly IStateMachineLocation                     _stateMachineLocation;

		public required IStateMachineValidator StateMachineValidator { private get; init; }

		public ScxmlLocationStateMachineGetter(IStateMachineLocation stateMachineLocation, ScxmlXmlResolver scxmlXmlResolver, Func<XmlReader, ValueTask<ScxmlDirector>> scxmlDirectorFactory)
		{
			_stateMachineLocation = stateMachineLocation;
			_scxmlXmlResolver = scxmlXmlResolver;
			_scxmlDirectorFactory = scxmlDirectorFactory;
		}

		public async ValueTask<IStateMachine> GetStateMachine()
		{
			using var xmlReader = CreateXmlReader();

			//var scxmlDirector = await _scxmlDirectorFactory(xmlReader).ConfigureAwait(false);
			var stateMachine =  await _ScxmlDeserializer.Deserialize(xmlReader).ConfigureAwait(false);

			StateMachineValidator.Validate(stateMachine);

			return stateMachine;
		}

		protected virtual XmlReader CreateXmlReader() => XmlReader.Create(_stateMachineLocation.Location.ToString(), GetXmlReaderSettings(), GetXmlParserContext());

		protected virtual XmlReaderSettings GetXmlReaderSettings() =>
			new()
			{
				Async = true,
				XmlResolver = _scxmlXmlResolver,
				DtdProcessing = DtdProcessing.Parse
			};

		protected virtual XmlParserContext GetXmlParserContext()
		{
			var nsManager = new XmlNamespaceManager(new NameTable());

			return new XmlParserContext(nt: null, nsManager, xmlLang: null, XmlSpace.None) { BaseURI = _stateMachineLocation.Location.ToString() };
		}
		/*
		private static ScxmlDirectorOptions GetScxmlDirectorOptions(ServiceLocator serviceLocator,
																	XmlParserContext xmlParserContext,
																	XmlReaderSettings xmlReaderSettings,
																	XmlResolver xmlResolver) =>
			new(serviceLocator)
			{
				NamespaceResolver = xmlParserContext.NamespaceManager,
				XmlReaderSettings = xmlReaderSettings,
				XmlResolver = xmlResolver,
				XIncludeAllowed = true,
				Async = true
			};*/
	}

	public class ScxmlStateMachineGetter
	{
		public ScxmlStateMachineGetter(IStateMachineLocation stateMachineLocation) { }

		public virtual IStateMachine GetStateMachine()
		{
			throw new NotImplementedException();
		}
	}

	public interface IScxmlStateMachine
	{
		TextReader CreateTextReader();
	}

	public interface ISourceStateMachine
	{
		Uri Location { get; }
	}

	public interface IHostBaseUri
	{
		Uri? HostBaseUri { get; }
	}

	public class StateMachineRunner : IStateMachineRunner, IDisposable
	{
		public required  IStateMachineHostContext _context { private get; init; }
		public required  IStateMachineController _controller { private get; init; }
		public required  IServiceProvider fg { private get; init; } //TODO:delete
		private readonly object                   _sync = new();
		private          bool                     _disposed;

	#region Interface IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

	#endregion

	#region Interface IStateMachineRunner

		public async ValueTask<IStateMachineController> Run(CancellationToken token)
		{
			//var startOptions = _serviceLocator.GetService<IStateMachineStartOptions>();
			//var errorProcessor = _serviceLocator.GetOptionalService<IErrorProcessor>();
			//var securityContext = _serviceLocator.GetOptionalService<ISecurityContext>();

			/*var controller = await _context.CreateAndAddStateMachine(_serviceLocator,
											   startOptions.SessionId, startOptions.Origin, startOptions.Parameters, (SecurityContext) securityContext,
											   new DeferredFinalizer(), errorProcessor, token)
										   .ConfigureAwait(false);*/

			//TODO: replace to IStateMachineController 
			//var controller = _serviceLocator.GetService<IStateMachineController>();

			lock (_sync)
			{
				if (_disposed)
				{
					throw new ObjectDisposedException(nameof(StateMachineRunner));
				}

				//Infra.Assert(_controller is null);

				_context.AddStateMachineController(_controller);
			}

			await _controller.StartAsync(token).ConfigureAwait(false);


			return _controller;
		}

		public async ValueTask Wait(CancellationToken token)
		{
			IStateMachineController? controller;

			lock (_sync)
			{
				if (_disposed)
				{
					return;
				}

				controller = _controller;
			}

			if (controller is not null)
			{
				await controller.GetResult(token).ConfigureAwait(false);
			}
		}

	#endregion

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (_sync)
				{
					if (_disposed)
					{
						return;
					}

					if (_controller is { } controller)
					{
						_context.RemoveStateMachineController(controller);
					}

					_disposed = true;
				}
			}
		}
	}

	public class ScopeManager : IScopeManager
	{
		private readonly ISecurityContext?    _securityContext;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IStateMachineHost    _stateMachineHost;
		private readonly IStateMachineHostContext    _stateMachineHostContext;

		public ScopeManager(ServiceLocator serviceLocator)
		{
			_serviceScopeFactory = serviceLocator.GetService<IServiceScopeFactory>();
			_securityContext = serviceLocator.GetOptionalService<ISecurityContext>();
			_stateMachineHost = serviceLocator.GetService<StateMachineHost>();
			_stateMachineHostContext = serviceLocator.GetService<StateMachineHostContext>();
		}

	#region Interface IScopeManager

		public virtual async ValueTask<IStateMachineController> RunStateMachine(IStateMachineStartOptions stateMachineStartOptions)
		{
			if (stateMachineStartOptions is null) throw new ArgumentNullException(nameof(stateMachineStartOptions));

			var scope = CreateStateMachineScope(stateMachineStartOptions);

			IStateMachineRunner? stateMachineRunner = default;
			try
			{
				stateMachineRunner = await scope.ServiceProvider.GetRequiredService<IStateMachineRunner>().ConfigureAwait(false);

				return await stateMachineRunner.Run(CancellationToken.None).ConfigureAwait(false);
			}
			finally
			{
				DisposeScopeOnComplete(stateMachineRunner, scope).Forget();
			}
		}

	#endregion

		private static async ValueTask DisposeScopeOnComplete(IStateMachineRunner? stateMachineRunner, IServiceScope serviceScope)
		{
			try
			{
				if (stateMachineRunner is not null)
				{
					await stateMachineRunner.Wait(CancellationToken.None).ConfigureAwait(false);
				}
			}
			finally
			{
				await serviceScope.DisposeAsync().ConfigureAwait(false);
			}
		}

		protected virtual IServiceScope CreateStateMachineScope(IStateMachineStartOptions stateMachineStartOptions)
		{
			if (stateMachineStartOptions is null) throw new ArgumentNullException(nameof(stateMachineStartOptions));

			var nestedSecurityContext = _securityContext is null ? SecurityContext.Create(SecurityContextType.NewTrustedStateMachine) :
				_securityContext.CreateNested(stateMachineStartOptions.SecurityContextType);

			switch (stateMachineStartOptions.Origin.Type)
			{
				case StateMachineOriginType.StateMachine:
				{
					var stateMachine = stateMachineStartOptions.Origin.AsStateMachine();
					return _serviceScopeFactory.CreateScope(
						services =>
						{
							services.AddForwarding(_ => stateMachine);
							services.AddForwarding(_ => stateMachineStartOptions);
							services.AddForwarding(_ => nestedSecurityContext);
							services.AddForwarding(_ => _stateMachineHost);
							services.AddForwarding(_ => _stateMachineHostContext);
							services.AddImplementation<StateMachineRunner>().For<IStateMachineRunner>();
						});
				}

				case StateMachineOriginType.Scxml:
				{
					var scxmlStateMachine = new ScxmlStateMachine(stateMachineStartOptions.Origin.AsScxml());
					var stateMachineLocation = stateMachineStartOptions.Origin.BaseUri is { } uri ? new StateMachineLocation(uri) : null;
					return _serviceScopeFactory.CreateScope(
						services =>
						{
							services.AddForwarding<IScxmlStateMachine>(_ => scxmlStateMachine);
							if (stateMachineLocation is not null)
							{
								services.AddForwarding<IStateMachineLocation>(_ => stateMachineLocation);
							}

							services.AddForwarding(_ => stateMachineStartOptions);
							services.AddForwarding(_ => nestedSecurityContext);
							services.AddForwarding(_ => _stateMachineHost);
							services.AddForwarding(_ => _stateMachineHostContext);
							services.AddImplementation<StateMachineRunner>().For<IStateMachineRunner>();
						});
				}

				case StateMachineOriginType.Source:
				{
					var location = stateMachineStartOptions.Origin.BaseUri.CombineWith(stateMachineStartOptions.Origin.AsSource());
					var stateMachineLocation = new StateMachineLocation(location);
					return _serviceScopeFactory.CreateScope(
						services =>
						{
							services.AddForwarding<IStateMachineLocation>(_ => stateMachineLocation);
							services.AddForwarding(_ => stateMachineStartOptions);
							services.AddForwarding(_ => nestedSecurityContext);
							services.AddForwarding(_ => _stateMachineHost);
							services.AddForwarding(_ => _stateMachineHostContext);
							services.AddImplementation<StateMachineRunner>().For<IStateMachineRunner>();
						});
					break;

					break;
				}
				default:
					throw new ArgumentException(Resources.Exception_StateMachineOriginMissed);
			}

			return _serviceScopeFactory.CreateScope(
				services =>
				{
					services.AddForwarding(_ => stateMachineStartOptions);
					services.AddForwarding(_ => nestedSecurityContext);
					services.AddForwarding(_ => _stateMachineHost);
				});
		}
	}

	public class ScxmlStateMachine : IScxmlStateMachine
	{
		private readonly string _scxml;

		public ScxmlStateMachine(string scxml)
		{
			_scxml = scxml;
		}

	#region Interface IScxmlStateMachine

		public TextReader CreateTextReader() => new StringReader(_scxml);

	#endregion
	}

	public class StateMachineLocation : IStateMachineLocation
	{
		private readonly Uri _location;

		public StateMachineLocation(Uri location)
		{
			_location = location;
		}

	#region Interface IStateMachineLocation

		public Uri Location => _location;

	#endregion
	}

	public interface IStateMachineArguments
	{
		DataModelValue Arguments { get; init; }
	}

	public interface IStateMachineStartOptions
	{
		SessionId           SessionId           { get; init; }
		StateMachineOrigin  Origin              { get; init; }
		DataModelValue      Parameters          { get; init; }
		SecurityContextType SecurityContextType { get; init; }
	}

	public record StateMachineSessionIdOld : IStateMachineSessionId
	{
		private readonly IStateMachineStartOptions _stateMachineStartOptions;

		public StateMachineSessionIdOld(IStateMachineStartOptions stateMachineStartOptions)
		{
			_stateMachineStartOptions = stateMachineStartOptions;
		}

	#region Interface IStateMachineSessionId

		public SessionId SessionId => _stateMachineStartOptions.SessionId;

	#endregion
	}

	public class StateMachineSessionId : IStateMachineSessionId
	{

	#region Interface IStateMachineSessionId

		public SessionId SessionId { get; } = SessionId.New();

	#endregion
	}

	public record StateMachineStartOptions : IStateMachineStartOptions
	{
	#region Interface IStateMachineStartOptions

		public SessionId           SessionId           { get; init; } = default!;
		public StateMachineOrigin  Origin              { get; init; }
		public DataModelValue      Parameters          { get; init; }
		public SecurityContextType SecurityContextType { get; init; }

	#endregion
	}

	//TODO:remove 1 at the end of class name
	[PublicAPI]
	public interface IErrorProcessorService1
	{
		void AddError(object? entity, string message, Exception? exception = default);
	}

	[PublicAPI]
	public interface IErrorProcessorService<TSource> : IErrorProcessorService1 { }

	[PublicAPI]
	public class ErrorProcessorService<TSource> : IErrorProcessorService<TSource>
	{
		public required IErrorProcessor ErrorProcessor { private get; init; }
		
		public required ILineInfoRequired? LineInfoRequired { private get; init; }

	#region Interface IErrorProcessorService1

		public virtual void AddError(object? entity, string message, Exception? exception = default)
		{
			Infra.Requires(message);

			if (LineInfoRequired?.LineInfoRequired ?? false)
			{
				if (entity.Is<IXmlLineInfo>(out var xmlLineInfo) && xmlLineInfo.HasLineInfo())
				{
					ErrorProcessor.AddError(new ErrorItem(typeof(TSource), message, exception, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition));

					return;
				}

				if (exception is XmlException { LineNumber: > 0 } xmlException)
				{
					ErrorProcessor.AddError(new ErrorItem(typeof(TSource), message, exception, xmlException.LineNumber, xmlException.LinePosition));

					return;
				}
			}

			ErrorProcessor.AddError(new ErrorItem(typeof(TSource), message, exception));
		}

	#endregion
	}

	public interface IDataModelHandlerService
	{
		ValueTask<IDataModelHandler> GetDataModelHandler(string? dataModelType);
	}

	public class DataModelHandlerService : IDataModelHandlerService
	{
		public required IAsyncEnumerable<IDataModelHandlerProvider>     DataModelHandlerProviders      { private get; init; }
		public required IErrorProcessorService<DataModelHandlerService> ErrorProcessorService          { private get; init; }
		public required Func<ValueTask<UnknownDataModelHandler>>        UnknownDataModelHandlerFactory { private get; init; }

	#region Interface IDataModelHandlerService

		public virtual async ValueTask<IDataModelHandler> GetDataModelHandler(string? dataModelType)
		{
			await foreach (var dataModelHandlerProvider in DataModelHandlerProviders.ConfigureAwait(false))
			{
				if (await dataModelHandlerProvider.TryGetDataModelHandler(dataModelType).ConfigureAwait(false) is { } dataModelHandler)
				{
					return dataModelHandler;
				}
			}

			ErrorProcessorService.AddError(entity: null, Res.Format(Resources.ErrorMessage_CantFindDataModelHandlerFactoryForDataModelType, dataModelType));

			return await UnknownDataModelHandlerFactory().ConfigureAwait(false);
		}

	#endregion
	}

	public interface IDataModelHandlerProvider
	{
		ValueTask<IDataModelHandler?> TryGetDataModelHandler(string? dataModelType);
	}

	public abstract class DataModelHandlerProviderBase<TDataModelHandler> : IDataModelHandlerProvider where TDataModelHandler : class, IDataModelHandler
	{
		public required Func<ValueTask<TDataModelHandler>> DataModelHandlerFactory { private get; init; }

	#region Interface IDataModelHandlerProvider

		public async ValueTask<IDataModelHandler?> TryGetDataModelHandler(string? dataModelType) =>
			CanHandle(dataModelType) ? await DataModelHandlerFactory().ConfigureAwait(false) : default;

	#endregion

		protected abstract bool CanHandle(string? dataModelType);
	}

	public class NullDataModelHandlerProvider : DataModelHandlerProviderBase<NullDataModelHandler>
	{
		protected override bool CanHandle(string? dataModelType) => dataModelType is null or "null";
	}

	public class RuntimeDataModelHandlerProvider : DataModelHandlerProviderBase<RuntimeDataModelHandler>
	{
		protected override bool CanHandle(string? dataModelType) => dataModelType == @"runtime";
	}

	public class XPathDataModelHandlerProvider : DataModelHandlerProviderBase<XPathDataModelHandler>
	{
		protected override bool CanHandle(string? dataModelType) => dataModelType == @"xpath";
	}
}