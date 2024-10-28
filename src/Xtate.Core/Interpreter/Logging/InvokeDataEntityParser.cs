// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Core;

public class InvokeDataEntityParser<TSource> : EntityParserBase<TSource, InvokeData>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(InvokeData invokeData)
	{
		Infra.Requires(invokeData);

		yield return new LoggingParameter(name: @"InvokeType", invokeData.Type);

		if (invokeData.Source is { } source)
		{
			yield return new LoggingParameter(name: @"InvokeSource", source);
		}
	}
}


public class InvokeDataVerboseEntityParser<TSource>() : EntityParserBase<TSource, InvokeData>(Level.Verbose)
{
	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	protected override IEnumerable<LoggingParameter> EnumerateProperties(InvokeData invokeData)
	{
		Infra.Requires(invokeData);

		if (invokeData.RawContent is { } rawContent)
		{
			yield return new LoggingParameter(name: @"RawContent", rawContent);
		}

		if (!invokeData.Content.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Content", invokeData.Content.ToObject()!);

			yield return new LoggingParameter(name: @"ContentText", DataModelHandler.ConvertToText(invokeData.Content));
		}

		if (!invokeData.Parameters.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Parameters", invokeData.Parameters.ToObject()!);

			yield return new LoggingParameter(name: @"ParametersText", DataModelHandler.ConvertToText(invokeData.Parameters));
		}
	}
}