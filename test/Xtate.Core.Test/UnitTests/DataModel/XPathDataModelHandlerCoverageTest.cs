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

using System.Runtime.CompilerServices;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataModel.XPath.Services;
using Xtate.DataTypes;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class XPathDataModelHandlerCoverageTest
{
	[TestMethod]
	public void HandlerUsesXPathSpecificFactoriesAndReportsUnsupportedScripts()
	{
		var errorProcessor = new Mock<IErrorProcessorService<XPathDataModelHandler>>();
		var forEachEvaluator = (XPathForEachEvaluator) RuntimeHelpers.GetUninitializedObject(typeof(XPathForEachEvaluator));
		var externalDataEvaluator = (XPathExternalDataExpressionEvaluator) RuntimeHelpers.GetUninitializedObject(typeof(XPathExternalDataExpressionEvaluator));
		var handler = CreateHandler(errorProcessor.Object, forEachEvaluator, externalDataEvaluator);
		IExecutableEntity executable = Mock.Of<IForEach>();
		var externalData = Mock.Of<IExternalDataExpression>();
		var script = Mock.Of<IScript>();

		((IDataModelHandler) handler).Process(ref executable);
		((IDataModelHandler) handler).Process(ref externalData);
		handler.Process(ref script);

		Assert.AreSame(forEachEvaluator, executable);
		Assert.AreSame(externalDataEvaluator, externalData);
		errorProcessor.Verify(processor => processor.AddError(script, It.IsAny<string>(), null), Times.Once);
	}

	[TestMethod]
	public void HandlerConvertsValuesToIndentedXml()
	{
		var handler = CreateHandler(Mock.Of<IErrorProcessorService<XPathDataModelHandler>>(), null!, null!);

		var text = handler.ConvertToText(new DataModelValue("value"));

		StringAssert.Contains(text, "value");
	}

	private static TestXPathDataModelHandler CreateHandler(IErrorProcessorService<XPathDataModelHandler> errorProcessor,
		XPathForEachEvaluator forEachEvaluator,
		XPathExternalDataExpressionEvaluator externalDataEvaluator) => new()
		{
			XPathErrorProcessorService = errorProcessor,
			XPathForEachEvaluatorFactory = _ => forEachEvaluator,
			XPathExternalDataExpressionEvaluatorFactory = _ => externalDataEvaluator,
			XPathContentBodyEvaluatorFactory = _ => null!,
			XPathInlineContentEvaluatorFactory = _ => null!,
			XPathValueExpressionEvaluatorFactory = (_, _) => null!,
			XPathConditionExpressionEvaluatorFactory = (_, _) => null!,
			XPathLocationExpressionEvaluatorFactory = (_, _) => null!,
			XPathCompiledExpressionFactory = (_, _) => null!,
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

	private sealed class TestXPathDataModelHandler : XPathDataModelHandler
	{
		public void Process(ref IScript script) => Visit(ref script);
	}
}
