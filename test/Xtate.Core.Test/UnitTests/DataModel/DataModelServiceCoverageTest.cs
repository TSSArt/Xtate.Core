// Copyright © 2019-2026 Sergii Artemenko
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

using System.Collections.Specialized;
using System.IO;
using System.Text;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.ResourceLoaders;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelServiceCoverageTest
{
	[TestMethod]
	public async Task ExternalDataExpressionEvaluatorsExposeAncestorUriAndLoadResourceContent()
	{
		var expression = new ExternalDataExpressionSource(new Uri("https://resource.test/data"));
		var evaluator = new TestExternalDataExpressionEvaluator(expression, new DataModelValue("base value"));

		Assert.AreSame(expression, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreSame(expression.Uri, evaluator.Uri);
		Assert.AreEqual("base value", DataModelValue.FromObject(await evaluator.EvaluateObject()).AsString());

		var resourceLoader = new Mock<IResourceLoader>();
		resourceLoader.Setup(static loader => loader.Request(It.IsAny<Uri>(), It.IsAny<NameValueCollection?>()))
					  .ReturnsAsync(new Resource(new MemoryStream(Encoding.UTF8.GetBytes("loaded content")), contentType: null));

		var defaultEvaluator = new DefaultExternalDataExpressionEvaluator(expression)
							   {
								   DataConverter = () => new ValueTask<DataConverter>(new DataConverter(caseSensitivity: null)),
								   ResourceLoader = () => new ValueTask<IResourceLoader>(resourceLoader.Object)
							   };

		Assert.AreEqual("loaded content", DataModelValue.FromObject(await defaultEvaluator.EvaluateObject()).AsString());
		resourceLoader.Verify(loader => loader.Request(expression.Uri!, null), Times.Once);
	}

	[TestMethod]
	public async Task DataModelHandlerServiceReturnsFirstMatchingProviderOrUnknownHandler()
	{
		var requestedType = "custom";
		var matchingHandler = new Mock<IDataModelHandler>().Object;
		var missProvider = new Mock<IDataModelHandlerProvider>();
		var hitProvider = new Mock<IDataModelHandlerProvider>();
		var errorProcessor = new Mock<IErrorProcessorService<DataModelHandlerService>>();

		missProvider.Setup(provider => provider.TryGetDataModelHandler(requestedType)).ReturnsAsync((IDataModelHandler?) null);
		hitProvider.Setup(provider => provider.TryGetDataModelHandler(requestedType)).ReturnsAsync(matchingHandler);

		var service = new DataModelHandlerService
					  {
						  DataModelHandlerProviders = ToAsyncEnumerable(missProvider.Object, hitProvider.Object),
						  ErrorProcessorService = errorProcessor.Object,
						  UnknownDataModelHandlerFactory = () => new ValueTask<UnknownDataModelHandler>(CreateUnknownHandler())
					  };

		Assert.AreSame(matchingHandler, await service.GetDataModelHandler(requestedType));
		errorProcessor.Verify(processor => processor.AddError(It.IsAny<object?>(), It.IsAny<string>(), It.IsAny<Exception?>()), Times.Never);

		var unknown = CreateUnknownHandler();
		var fallbackService = new DataModelHandlerService
							  {
								  DataModelHandlerProviders = ToAsyncEnumerable(missProvider.Object),
								  ErrorProcessorService = errorProcessor.Object,
								  UnknownDataModelHandlerFactory = () => new ValueTask<UnknownDataModelHandler>(unknown)
							  };

		Assert.AreSame(unknown, await fallbackService.GetDataModelHandler(requestedType));
		errorProcessor.Verify(processor => processor.AddError(null, It.Is<string>(message => message.Contains(requestedType, StringComparison.Ordinal)), null), Times.Once);
	}

	[TestMethod]
	public void UnknownDataModelHandlerReportsExecutableProcessingError()
	{
		var errorProcessor = new Mock<IErrorProcessorService<UnknownDataModelHandler>>();
		var handler = CreateUnknownHandler(errorProcessor.Object);
		IExecutableEntity executable = new ExecutableEntitySource();

		((IDataModelHandler) handler).Process(ref executable);

		errorProcessor.Verify(processor => processor.AddError(
								  executable,
								  It.Is<string>(message => !string.IsNullOrEmpty(message)),
								  null),
							  Times.Once);
	}

	private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
	{
		foreach (var item in items)
		{
			yield return item;
			await Task.Yield();
		}
	}

	private static UnknownDataModelHandler CreateUnknownHandler(IErrorProcessorService<UnknownDataModelHandler>? errorProcessor = null)
	{
		errorProcessor ??= Mock.Of<IErrorProcessorService<UnknownDataModelHandler>>();

		return new UnknownDataModelHandler
			   {
				   UnknownErrorProcessorService = errorProcessor,
				   DefaultLogEvaluatorFactory = _ => null!,
				   DefaultSendEvaluatorFactory = _ => null!,
				   DefaultCancelEvaluatorFactory = _ => null!,
				   DefaultIfEvaluatorFactory = _ => null!,
				   DefaultRaiseEvaluatorFactory = _ => null!,
				   DefaultForEachEvaluatorFactory = _ => null!,
				   DefaultAssignEvaluatorFactory = _ => null!,
				   DefaultScriptEvaluatorFactory = _ => null!,
				   DefaultCustomActionEvaluatorFactory = _ => null!,
				   DefaultContentBodyEvaluatorFactory = _ => null!,
				   DefaultInlineContentEvaluatorFactory = _ => null!,
				   DefaultExternalDataExpressionEvaluatorFactory = _ => null!,
				   CustomActionContainerFactory = _ => null!
			   };
	}

	private sealed class ExternalDataExpressionSource(Uri uri) : IExternalDataExpression
	{
		public Uri? Uri => uri;
	}

	private sealed class TestExternalDataExpressionEvaluator(IExternalDataExpression expression, IObject value) : ExternalDataExpressionEvaluator(expression)
	{
		public override ValueTask<IObject> EvaluateObject() => new(value);
	}

	private sealed class ExecutableEntitySource : IExecutableEntity;
}
