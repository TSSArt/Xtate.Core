﻿// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.DataModel;

public class DataConverter(ICaseSensitivity? caseSensitivity)
{
	private readonly bool _caseInsensitive = caseSensitivity?.CaseInsensitive ?? false;

	public static ImmutableArray<Param> AsParamArray(ImmutableArray<IParam> parameters) => !parameters.IsDefault ? ImmutableArray.CreateRange(parameters, param => new Param(param)) : default;

	public ValueTask<DataModelValue> GetData(IValueEvaluator? contentBodyEvaluator,
											 IObjectEvaluator? contentExpressionEvaluator,
											 ImmutableArray<ILocationEvaluator> nameEvaluatorList,
											 ImmutableArray<Param> parameterList)
	{
		if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
		{
			return GetContent(contentBodyEvaluator, contentExpressionEvaluator);
		}

		return GetParameters(nameEvaluatorList, parameterList);
	}

	public async ValueTask<DataModelValue> GetContent(IValueEvaluator? contentBodyEvaluator, IObjectEvaluator? contentExpressionEvaluator)
	{
		if (contentExpressionEvaluator is not null)
		{
			var obj = await contentExpressionEvaluator.EvaluateObject().ConfigureAwait(false);

			return DataModelValue.FromObject(obj).AsConstant();
		}

		if (contentBodyEvaluator is IObjectEvaluator objectEvaluator)
		{
			var obj = await objectEvaluator.EvaluateObject().ConfigureAwait(false);

			return DataModelValue.FromObject(obj).AsConstant();
		}

		if (contentBodyEvaluator is IStringEvaluator stringEvaluator)
		{
			var str = await stringEvaluator.EvaluateString().ConfigureAwait(false);

			return new DataModelValue(str);
		}

		return default;
	}

	public async ValueTask<DataModelValue> GetParameters(ImmutableArray<ILocationEvaluator> nameEvaluatorList,
														 ImmutableArray<Param> parameterList)
	{
		if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
		{
			return default;
		}

		var attributes = new DataModelList(_caseInsensitive);

		if (!nameEvaluatorList.IsDefaultOrEmpty)
		{
			foreach (var locationEvaluator in nameEvaluatorList)
			{
				var name = await locationEvaluator.GetName().ConfigureAwait(false);
				var value = await locationEvaluator.GetValue().ConfigureAwait(false);

				attributes.Add(name, DataModelValue.FromObject(value).AsConstant());
			}
		}

		if (!parameterList.IsDefaultOrEmpty)
		{
			foreach (var param in parameterList)
			{
				var value = DefaultObject.Null;

				if (param.ExpressionEvaluator is { } expressionEvaluator)
				{
					value = await expressionEvaluator.EvaluateObject().ConfigureAwait(false);
				}
				else if (param.LocationEvaluator is { } locationEvaluator)
				{
					value = await locationEvaluator.GetValue().ConfigureAwait(false);
				}

				attributes.Add(param.Name, DataModelValue.FromObject(value).AsConstant());
			}
		}

		return new DataModelValue(attributes);
	}

	public async ValueTask<DataModelValue> FromContent(Resource resource)
	{
		Infra.Requires(resource);

		if (await resource.GetContent().ConfigureAwait(false) is { } content)
		{
			return new DataModelValue(content);
		}

		return DataModelValue.Null;
	}

	public DataModelValue FromEvent(IIncomingEvent incomingEvent)
	{
		var eventList = new DataModelList(_caseInsensitive)
						{
							{ @"name", incomingEvent.Name.ToString() },
							{ @"type", GetTypeString(incomingEvent.Type) },
							{ @"sendid", incomingEvent.SendId },
							{ @"origin", incomingEvent.Origin?.ToString() },
							{ @"origintype", incomingEvent.OriginType?.ToString() },
							{ @"invokeid", incomingEvent.InvokeId },
							{ @"data", incomingEvent.Data.AsConstant() }
						};

		eventList.MakeDeepConstant();

		return eventList;

		static string GetTypeString(EventType eventType) =>
			eventType switch
			{
				EventType.Platform => @"platform",
				EventType.Internal => @"internal",
				EventType.External => @"external",
				_                  => throw Infra.Unmatched(eventType)
			};
	}

	public DataModelValue FromException(Exception exception)
	{
		Infra.Requires(exception);

		return LazyValue.Create(exception, _caseInsensitive, ValueFactory);

		static DataModelValue ValueFactory(Exception exception, bool caseInsensitive)
		{
			var exceptionData = new DataModelList(caseInsensitive)
								{
									{ @"message", exception.Message },
									{ @"typeName", exception.GetType().Name },
									{ @"source", exception.Source },
									{ @"typeFullName", exception.GetType().FullName },
									{ @"stackTrace", exception.StackTrace },
									{ @"text", exception.ToString() }
								};

			exceptionData.MakeDeepConstant();

			return new DataModelValue(exceptionData);
		}
	}

	public readonly struct Param(IParam param)
	{
		public string Name { get; } = param.Name!;

		public IObjectEvaluator? ExpressionEvaluator { get; } = param.Expression?.As<IObjectEvaluator>();

		public ILocationEvaluator? LocationEvaluator { get; } = param.Location?.As<ILocationEvaluator>();
	}
}