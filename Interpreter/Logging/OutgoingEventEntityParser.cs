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

public class OutgoingEventEntityParser<TSource> : EntityParserBase<TSource, IOutgoingEvent>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(IOutgoingEvent evt)
	{
		if (!evt.NameParts.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"EventName", EventName.ToName(evt.NameParts));
		}

		if (evt.SendId is { } sendId)
		{
			yield return new LoggingParameter(name: @"SendId", sendId);
		}

		if (evt.Type is { } type)
		{
			yield return new LoggingParameter(name: @"EventType", type);
		}

		if (evt.Target is { } target)
		{
			yield return new LoggingParameter(name: @"Target", target);
		}

		if (evt.DelayMs is var delayMs and > 0)
		{
			yield return new LoggingParameter(name: @"DelayMs", delayMs);
		}
	}
}

public class OutgoingEventVerboseEntityParser<TSource>() : EntityParserBase<TSource, IOutgoingEvent>(Level.Verbose)
{
	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	protected override IEnumerable<LoggingParameter> EnumerateProperties(IOutgoingEvent evt)
	{
		if (!evt.Data.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Data", evt.Data.ToObject());

			yield return new LoggingParameter(name: @"DataText", DataModelHandler.ConvertToText(evt.Data));
		}
	}
}