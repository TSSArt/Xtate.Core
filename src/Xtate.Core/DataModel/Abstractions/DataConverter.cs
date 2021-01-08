#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	internal static class DataConverter
	{
		public static ValueTask<DataModelValue> GetData(IValueEvaluator? contentBodyEvaluator, IObjectEvaluator? contentExpressionEvaluator, ImmutableArray<ILocationEvaluator> nameEvaluatorList,
														ImmutableArray<DefaultParam> parameterList, IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
			{
				return GetContent(contentBodyEvaluator, contentExpressionEvaluator, executionContext, token);
			}

			return GetParameters(nameEvaluatorList, parameterList, executionContext, token);
		}

		public static async ValueTask<DataModelValue> GetContent(IValueEvaluator? contentBodyEvaluator, IObjectEvaluator? contentExpressionEvaluator,
																 IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			if (contentExpressionEvaluator is not null)
			{
				var obj = await contentExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj).AsConstant();
			}

			if (contentBodyEvaluator is IObjectEvaluator objectEvaluator)
			{
				var obj = await objectEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj).AsConstant();
			}

			if (contentBodyEvaluator is IStringEvaluator stringEvaluator)
			{
				var str = await stringEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false);

				return new DataModelValue(str);
			}

			return default;
		}

		public static async ValueTask<DataModelValue> GetParameters(ImmutableArray<ILocationEvaluator> nameEvaluatorList, ImmutableArray<DefaultParam> parameterList,
																	IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
			{
				return default;
			}

			var attributes = new DataModelList(executionContext.DataModel.CaseInsensitive);

			if (!nameEvaluatorList.IsDefaultOrEmpty)
			{
				foreach (var locationEvaluator in nameEvaluatorList)
				{
					var name = locationEvaluator.GetName(executionContext);
					var value = await locationEvaluator.GetValue(executionContext, token).ConfigureAwait(false);

					attributes.Add(name, DataModelValue.FromObject(value).AsConstant());
				}
			}

			if (!parameterList.IsDefaultOrEmpty)
			{
				foreach (var param in parameterList)
				{
					var name = param.Name;
					var value = await GetParamValue(param, executionContext, token).ConfigureAwait(false);

					attributes.Add(name, DataModelValue.FromObject(value).AsConstant());
				}
			}

			return new DataModelValue(attributes);
		}

		private static async ValueTask<IObject?> GetParamValue(DefaultParam param, IExecutionContext executionContext, CancellationToken token)
		{
			if (param.ExpressionEvaluator is not null)
			{
				return await param.ExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);
			}

			if (param.LocationEvaluator is not null)
			{
				return await param.LocationEvaluator.GetValue(executionContext, token).ConfigureAwait(false);
			}

			return null;
		}

		public static DataModelValue FromContent(string content, ContentType? contentType)
		{
			var _ = contentType;

			return new DataModelValue(content);
		}

		public static DataModelValue FromEvent(IEvent evt, bool caseInsensitive)
		{
			if (evt is null) throw new ArgumentNullException(nameof(evt));

			var eventList = new DataModelList(caseInsensitive)
							{
									{ @"name", EventName.ToName(evt.NameParts) },
									{ @"type", GetTypeString(evt.Type) },
									{ @"sendid", evt.SendId },
									{ @"origin", evt.Origin?.ToString() },
									{ @"origintype", evt.OriginType?.ToString() },
									{ @"invokeid", evt.InvokeId },
									{ @"data", evt.Data.AsConstant() }
							};

			eventList.MakeDeepConstant();

			return new DataModelValue(eventList);

			static string GetTypeString(EventType eventType)
			{
				return eventType switch
				{
						EventType.Platform => @"platform",
						EventType.Internal => @"internal",
						EventType.External => @"external",
						_ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, message: null)
				};
			}
		}

		public static DataModelValue FromException(Exception exception, bool caseInsensitive)
		{
			if (exception is null) throw new ArgumentNullException(nameof(exception));

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
}