using System;
using System.Collections.Immutable;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel
{
	internal static class DataConverter
	{
		public static ValueTask<DataModelValue> GetData(IValueEvaluator? contentBodyEvaluator, IObjectEvaluator? contentExpressionEvaluator, ImmutableArray<ILocationEvaluator> nameEvaluatorList,
														ImmutableArray<DefaultParam> parameterList, IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
			{
				return GetContent(contentBodyEvaluator, contentExpressionEvaluator, executionContext, token);
			}

			return GetParameters(nameEvaluatorList, parameterList, executionContext, token);
		}

		public static async ValueTask<DataModelValue> GetContent(IValueEvaluator? contentBodyEvaluator, IObjectEvaluator? contentExpressionEvaluator,
																 IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			if (contentExpressionEvaluator != null)
			{
				var obj = await contentExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj.ToObject()).AsConstant();
			}

			if (contentBodyEvaluator is IObjectEvaluator objectEvaluator)
			{
				var obj = await objectEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj.ToObject()).AsConstant();
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
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
			{
				return default;
			}

			var attributes = new DataModelObject(executionContext.DataModel.CaseInsensitive);

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
					object? value = null;

					if (param.ExpressionEvaluator != null)
					{
						value = (await param.ExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false)).ToObject();
					}
					else if (param.LocationEvaluator != null)
					{
						value = await param.LocationEvaluator.GetValue(executionContext, token).ConfigureAwait(false);
					}

					attributes.Add(name, DataModelValue.FromObject(value).AsConstant());
				}
			}

			return new DataModelValue(attributes);
		}

		public static DataModelValue FromContent(string content, ContentType? contentType)
		{
			var _ = contentType;

			return new DataModelValue(content);
		}

		public static DataModelValue FromEvent(IEvent evt, bool caseInsensitive)
		{
			if (evt == null) throw new ArgumentNullException(nameof(evt));

			var eventObject = new DataModelObject(caseInsensitive)
							  {
									  { @"name", EventName.ToName(evt.NameParts) },
									  { @"type", GetTypeString(evt.Type) },
									  { @"sendid", evt.SendId },
									  { @"origin", evt.Origin?.ToString() },
									  { @"origintype", evt.OriginType?.ToString() },
									  { @"invokeid", evt.InvokeId },
									  { @"data", evt.Data.AsConstant() }
							  };

			eventObject.MakeDeepConstant();

			return new DataModelValue(eventObject);

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
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			var exceptionData = new DataModelObject(caseInsensitive)
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