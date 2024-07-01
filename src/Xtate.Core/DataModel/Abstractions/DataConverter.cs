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

<<<<<<< Updated upstream
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xtate.Core;
=======
namespace Xtate.DataModel;
>>>>>>> Stashed changes

public class DataConverter(IDataModelHandler? dataModelHandler)
{
<<<<<<< Updated upstream
	public class DataConverter
	{
		public struct Param
		{
			public Param(IParam param)
			{
				Name = param.Name;
				ExpressionEvaluator = param.Expression?.As<IObjectEvaluator>();
				LocationEvaluator = param.Location?.As<ILocationEvaluator>();
			}

			public string              Name                { get; }
			public IObjectEvaluator?   ExpressionEvaluator { get; }
			public ILocationEvaluator? LocationEvaluator   { get; }
		}

		public static ImmutableArray<Param> AsParamArray(ImmutableArray<IParam> parameters)
		{
			if (parameters.IsDefault)
			{
				return default;
			}

			return ImmutableArray.CreateRange(parameters, param => new Param(param));
		}

		private readonly bool _caseInsensitive;

		public DataConverter(IDataModelHandler? dataModelHandler)
		{
			_caseInsensitive = dataModelHandler?.CaseInsensitive ?? false;
		}

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

		public async ValueTask<DataModelValue> GetContent(IValueEvaluator? contentBodyEvaluator,
														  IObjectEvaluator? contentExpressionEvaluator)
		{
			if (contentExpressionEvaluator is not null)
			{
				var obj = await contentExpressionEvaluator.EvaluateObject().ConfigureAwait(false);
=======
	private readonly bool _caseInsensitive = dataModelHandler?.CaseInsensitive ?? false;

	public static ImmutableArray<Param> AsParamArray(ImmutableArray<IParam> parameters)
	{
		return !parameters.IsDefault
			? ImmutableArray.CreateRange(parameters, param => new Param(param))
			: default;
	}

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
>>>>>>> Stashed changes

		if (contentBodyEvaluator is IObjectEvaluator objectEvaluator)
		{
			var obj = await objectEvaluator.EvaluateObject().ConfigureAwait(false);

<<<<<<< Updated upstream
			if (contentBodyEvaluator is IObjectEvaluator objectEvaluator)
			{
				var obj = await objectEvaluator.EvaluateObject().ConfigureAwait(false);
=======
			return DataModelValue.FromObject(obj).AsConstant();
		}
>>>>>>> Stashed changes

		if (contentBodyEvaluator is IStringEvaluator stringEvaluator)
		{
			var str = await stringEvaluator.EvaluateString().ConfigureAwait(false);

<<<<<<< Updated upstream
			if (contentBodyEvaluator is IStringEvaluator stringEvaluator)
			{
				var str = await stringEvaluator.EvaluateString().ConfigureAwait(false);
=======
			return new DataModelValue(str);
		}
>>>>>>> Stashed changes

		return default;
	}

	public async ValueTask<DataModelValue> GetParameters(ImmutableArray<ILocationEvaluator> nameEvaluatorList,
														 ImmutableArray<Param> parameterList)
	{
		if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
		{
			return default;
		}

<<<<<<< Updated upstream
		public async ValueTask<DataModelValue> GetParameters(ImmutableArray<ILocationEvaluator> nameEvaluatorList,
															 ImmutableArray<Param> parameterList)
		{
			if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
=======
		var attributes = new DataModelList(_caseInsensitive);

		if (!nameEvaluatorList.IsDefaultOrEmpty)
		{
			foreach (var locationEvaluator in nameEvaluatorList)
>>>>>>> Stashed changes
			{
				var name = await locationEvaluator.GetName().ConfigureAwait(false);
				var value = await locationEvaluator.GetValue().ConfigureAwait(false);

				attributes.Add(name, DataModelValue.FromObject(value).AsConstant());
			}
<<<<<<< Updated upstream

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
					else if (param.LocationEvaluator is  { } locationEvaluator)
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

		public DataModelValue FromEvent(IEvent evt)
		{
			Infra.Requires(evt);

			var eventList = new DataModelList(_caseInsensitive)
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

			return eventList;

			static string GetTypeString(EventType eventType)
			{
				return eventType switch
					   {
						   EventType.Platform => @"platform",
						   EventType.Internal => @"internal",
						   EventType.External => @"external",
						   _                  => Infra.Unexpected<string>(eventType)
					   };
			}
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
=======
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

	public DataModelValue FromEvent(IEvent evt)
	{
		Infra.Requires(evt);

		var eventList = new DataModelList(_caseInsensitive)
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

		return eventList;

		static string GetTypeString(EventType eventType)
		{
			return eventType switch
				   {
					   EventType.Platform => @"platform",
					   EventType.Internal => @"internal",
					   EventType.External => @"external",
					   _                  => Infra.Unexpected<string>(eventType)
				   };
>>>>>>> Stashed changes
		}
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