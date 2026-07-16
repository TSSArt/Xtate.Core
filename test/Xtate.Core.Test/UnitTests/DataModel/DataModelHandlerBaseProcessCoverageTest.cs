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

using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class DataModelHandlerBaseProcessCoverageTest
{
	[TestMethod]
	public void BaseHandlerExposesDefaultsAndProcessesEveryPublicEntityCategory()
	{
		var handler = CreateHandler();
		var contract = (IDataModelHandler) handler;
		IExecutableEntity executable = new LogSource();
		IValueExpression value = new ValueSource();
		ILocationExpression location = new LocationSource();
		IConditionExpression condition = new ConditionSource();
		IContentBody contentBody = new ContentBodySource();
		IInlineContent inlineContent = new InlineContentSource();
		IExternalDataExpression externalData = new ExternalDataSource();

		contract.Process(ref executable);
		contract.Process(ref value);
		contract.Process(ref location);
		contract.Process(ref condition);
		contract.Process(ref contentBody);
		contract.Process(ref inlineContent);
		contract.Process(ref externalData);

		Assert.IsInstanceOfType<LogSource>(executable);
		Assert.IsInstanceOfType<ValueSource>(value);
		Assert.IsInstanceOfType<LocationSource>(location);
		Assert.IsInstanceOfType<ConditionSource>(condition);
		Assert.IsInstanceOfType<ContentBodySource>(contentBody);
		Assert.IsInstanceOfType<InlineContentSource>(inlineContent);
		Assert.IsInstanceOfType<ExternalDataSource>(externalData);
		Assert.AreEqual(expected: 1, handler.LogVisits);
		Assert.AreEqual(expected: 1, handler.ContentBodyVisits);
		Assert.AreEqual(expected: 1, handler.InlineContentVisits);
		Assert.AreEqual(expected: 1, handler.ExternalDataVisits);
		Assert.AreEqual(expected: "text", contract.ConvertToText(new DataModelValue("text")));
		Assert.IsFalse(contract.CaseInsensitive);
		Assert.IsEmpty(contract.DataModelVars);
	}

	[TestMethod]
	public void BaseHandlerUsesDefaultExternalDataExpressionFactory()
	{
		var handler = CreateBaseExternalHandler();
		IExternalDataExpression externalData = new ExternalDataSource();

		((IDataModelHandler) handler).Process(ref externalData);

		Assert.IsInstanceOfType<DefaultExternalDataExpressionEvaluator>(externalData);
	}

	private static TestHandler CreateHandler() =>
		new()
		{
			DefaultLogEvaluatorFactory = null!,
			DefaultSendEvaluatorFactory = null!,
			DefaultCancelEvaluatorFactory = null!,
			DefaultIfEvaluatorFactory = null!,
			DefaultRaiseEvaluatorFactory = null!,
			DefaultForEachEvaluatorFactory = null!,
			DefaultAssignEvaluatorFactory = null!,
			DefaultScriptEvaluatorFactory = null!,
			DefaultCustomActionEvaluatorFactory = null!,
			DefaultContentBodyEvaluatorFactory = null!,
			DefaultInlineContentEvaluatorFactory = null!,
			DefaultExternalDataExpressionEvaluatorFactory = null!,
			CustomActionContainerFactory = null!
		};

	private static BaseExternalHandler CreateBaseExternalHandler() =>
		new()
		{
			DefaultLogEvaluatorFactory = null!,
			DefaultSendEvaluatorFactory = null!,
			DefaultCancelEvaluatorFactory = null!,
			DefaultIfEvaluatorFactory = null!,
			DefaultRaiseEvaluatorFactory = null!,
			DefaultForEachEvaluatorFactory = null!,
			DefaultAssignEvaluatorFactory = null!,
			DefaultScriptEvaluatorFactory = null!,
			DefaultCustomActionEvaluatorFactory = null!,
			DefaultContentBodyEvaluatorFactory = null!,
			DefaultInlineContentEvaluatorFactory = null!,
			DefaultExternalDataExpressionEvaluatorFactory = source => new DefaultExternalDataExpressionEvaluator(source)
																	  {
																		  DataConverter = null!,
																		  ResourceLoader = null!
																	  },
			CustomActionContainerFactory = null!
		};

	private sealed class BaseExternalHandler : DataModelHandlerBase;

	private sealed class TestHandler : DataModelHandlerBase
	{
		public int LogVisits { get; private set; }

		public int ContentBodyVisits { get; private set; }

		public int InlineContentVisits { get; private set; }

		public int ExternalDataVisits { get; private set; }

		protected override ILog GetEvaluator(ILog log)
		{
			LogVisits ++;

			return log;
		}

		protected override IContentBody GetEvaluator(IContentBody contentBody)
		{
			ContentBodyVisits ++;

			return contentBody;
		}

		protected override IInlineContent GetEvaluator(IInlineContent inlineContent)
		{
			InlineContentVisits ++;

			return inlineContent;
		}

		protected override IExternalDataExpression GetEvaluator(IExternalDataExpression externalDataExpression)
		{
			ExternalDataVisits ++;

			return externalDataExpression;
		}
	}

	private sealed class LogSource : ILog
	{
	#region Interface ILog

		public string? Label => "label";

		public IValueExpression? Expression => null;

	#endregion
	}

	private sealed class ValueSource : IValueExpression
	{
	#region Interface IValueExpression

		public string? Expression => "value";

	#endregion
	}

	private sealed class LocationSource : ILocationExpression
	{
	#region Interface ILocationExpression

		public string? Expression => "location";

	#endregion
	}

	private sealed class ConditionSource : IConditionExpression
	{
	#region Interface IConditionExpression

		public string? Expression => "condition";

	#endregion
	}

	private sealed class ContentBodySource : IContentBody
	{
	#region Interface IContentBody

		public string? Value => "body";

	#endregion
	}

	private sealed class InlineContentSource : IInlineContent
	{
	#region Interface IInlineContent

		public string? Value => "inline";

	#endregion
	}

	private sealed class ExternalDataSource : IExternalDataExpression
	{
	#region Interface IExternalDataExpression

		public Uri? Uri => new("https://example.test/data");

	#endregion
	}
}