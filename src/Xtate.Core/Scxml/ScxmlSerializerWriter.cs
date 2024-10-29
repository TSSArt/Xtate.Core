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

using System.Xml;

namespace Xtate.Scxml;

public class ScxmlSerializerWriter(XmlWriter writer) : StateMachineVisitor
{
	private const string ScxmlNs = "http://www.w3.org/2005/07/scxml";

	private const string XtateScxmlNs = "http://xtate.net/scxml";

	private const string Space = " ";

	public void Serialize(IStateMachine stateMachine) => Visit(ref stateMachine);

	protected override void Visit(ref IStateMachine entity)
	{
		Infra.NotNull(entity);

		writer.WriteStartElement(prefix: "", localName: "scxml", ScxmlNs);
		writer.WriteAttributeString(localName: "version", value: @"1.0");

		if (entity.DataModelType is { } dataModelType)
		{
			writer.WriteAttributeString(localName: "datamodel", dataModelType);
		}

		var target = entity.Initial?.Transition?.Target ?? default;

		if (!target.IsDefaultOrEmpty)
		{
			writer.WriteStartAttribute("initial");
			WriteArray(target, i => i.Value, Space);
			writer.WriteEndAttribute();
		}

		if (entity.Is<IStateMachineOptions>(out var options))
		{
			WriteOptions(options);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Build(ref StateMachineEntity properties)
	{
		properties.Initial = default;

		base.Build(ref properties);
	}

	private void WriteOptions(IStateMachineOptions options)
	{
		if (options.PersistenceLevel is { } persistenceLevel)
		{
			writer.WriteAttributeString(localName: "persistence", XtateScxmlNs, persistenceLevel.ToString());
		}

		if (options.SynchronousEventProcessing is { } synchronousEventProcessing)
		{
			writer.WriteAttributeString(localName: "synchronous", XtateScxmlNs, XmlConvert.ToString(synchronousEventProcessing));
		}

		if (options.ExternalQueueSize is { } externalQueueSize)
		{
			writer.WriteAttributeString(localName: "queueSize", XtateScxmlNs, XmlConvert.ToString(externalQueueSize));
		}

		if (options.UnhandledErrorBehaviour is { } unhandledErrorBehaviour)
		{
			writer.WriteAttributeString(localName: "onError", XtateScxmlNs, unhandledErrorBehaviour.ToString());
		}
	}

	protected override void Visit(ref IInitial entity)
	{
		writer.WriteStartElement("initial");

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IState entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("state");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IParallel entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("parallel");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IHistory entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("history");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
		}

		if (entity.Type != HistoryType.Shallow)
		{
			writer.WriteAttributeString(localName: "type", value: @"deep");
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IFinal entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("final");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref ITransition entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("transition");

		if (entity.Type != TransitionType.External)
		{
			writer.WriteAttributeString(localName: "type", value: @"internal");
		}

		if (!entity.EventDescriptors.IsDefaultOrEmpty)
		{
			writer.WriteStartAttribute("event");
			WriteArray(entity.EventDescriptors, ed => ed.Value, Space);
			writer.WriteEndAttribute();
		}

		var condition = entity.Condition?.As<IConditionExpression>().Expression;

		if (condition is not null)
		{
			writer.WriteAttributeString(localName: "cond", condition);
		}

		if (!entity.Target.IsDefaultOrEmpty)
		{
			writer.WriteStartAttribute("target");
			WriteArray(entity.Target, i => i.Value, Space);
			writer.WriteEndAttribute();
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	private void WriteArray<T>(ImmutableArray<T> array, Func<T, string> converter, string delimiter)
	{
		var writeDelimiter = false;

		foreach (var item in array)
		{
			if (writeDelimiter)
			{
				writer.WriteString(delimiter);
			}

			writer.WriteString(converter(item));

			writeDelimiter = true;
		}
	}

	protected override void Visit(ref ISend entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("send");

		if (entity.EventName is { } eventName)
		{
			writer.WriteAttributeString(localName: "event", eventName);
		}

		if (entity.EventExpression?.Expression is { } expression)
		{
			writer.WriteAttributeString(localName: "eventexpr", expression);
		}

		if (entity.Target is { } target)
		{
			writer.WriteAttributeString(localName: "target", target.ToString());
		}

		if (entity.TargetExpression?.Expression is { } targetExpression)
		{
			writer.WriteAttributeString(localName: "targetexpr", targetExpression);
		}

		if (entity.Type is { } type)
		{
			writer.WriteAttributeString(localName: "type", type.ToString());
		}

		if (entity.TypeExpression?.Expression is { } typeExpression)
		{
			writer.WriteAttributeString(localName: "typeexpr", typeExpression);
		}

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id);
		}

		if (entity.IdLocation?.Expression is { } idLocation)
		{
			writer.WriteAttributeString(localName: "idlocation", idLocation);
		}

		if (entity.DelayMs is { } ms)
		{
			var delayStr = ms % 1000 == 0 ? ms / 1000 + @"s" : ms + @"ms";
			writer.WriteAttributeString(localName: "delay", delayStr);
		}

		if (entity.DelayExpression?.Expression is { } delayExpression)
		{
			writer.WriteAttributeString(localName: "delayexpr", delayExpression);
		}

		if (!entity.NameList.IsDefaultOrEmpty)
		{
			writer.WriteStartAttribute("namelist");
			WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
			writer.WriteEndAttribute();
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IScript entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("script");

		if (entity.Source?.Uri is { } source)
		{
			writer.WriteAttributeString(localName: "src", source.ToString());
		}

		if (entity.Content?.Expression is { } content)
		{
			writer.WriteRaw(content);
		}

		writer.WriteEndElement();
	}

	protected override void Visit(ref ICustomAction entity)
	{
		Infra.Requires(entity);

		if (entity.Xml is { } xml)
		{
			writer.WriteRaw(xml);
		}
	}

	protected override void Visit(ref IRaise entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("raise");

		var nameParts = entity.OutgoingEvent?.NameParts ?? default;

		if (!nameParts.IsDefaultOrEmpty)
		{
			writer.WriteStartAttribute("event");
			EventName.WriteXml(writer, nameParts);
			writer.WriteEndAttribute();
		}

		writer.WriteEndElement();
	}

	protected override void Visit(ref ILog entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("log");

		if (entity.Label is { } label)
		{
			writer.WriteAttributeString(localName: "label", label);
		}

		if (entity.Expression?.Expression is { } expression)
		{
			writer.WriteAttributeString(localName: "expr", expression);
		}

		writer.WriteEndElement();
	}

	protected override void Visit(ref IIf entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("if");

		if (entity.Condition?.Expression is { } condition)
		{
			writer.WriteAttributeString(localName: "cond", condition);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IForEach entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("foreach");

		if (entity.Array?.Expression is { } array)
		{
			writer.WriteAttributeString(localName: "array", array);
		}

		if (entity.Item?.Expression is { } item)
		{
			writer.WriteAttributeString(localName: "item", item);
		}

		if (entity.Index?.Expression is { } index)
		{
			writer.WriteAttributeString(localName: "index", index);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IElseIf entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("elseif");

		if (entity.Condition?.Expression is { } condition)
		{
			writer.WriteAttributeString(localName: "cond", condition);
		}

		writer.WriteEndElement();
	}

	protected override void Visit(ref IElse entity)
	{
		writer.WriteStartElement("else");
		writer.WriteEndElement();
	}

	protected override void Visit(ref ICancel entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("cancel");

		if (entity.SendId is { } sendId)
		{
			writer.WriteAttributeString(localName: "sendid", sendId);
		}

		if (entity.SendIdExpression?.Expression is { } senIdExpression)
		{
			writer.WriteAttributeString(localName: "sendidexpr", senIdExpression);
		}

		writer.WriteEndElement();
	}

	protected override void Visit(ref IAssign entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("assign");

		if (entity.Location?.Expression is { } location)
		{
			writer.WriteAttributeString(localName: "location", location);
		}

		if (entity.Expression?.Expression is { } expression)
		{
			writer.WriteAttributeString(localName: "expr", expression);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IDataModel entity)
	{
		writer.WriteStartElement("datamodel");

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IOnExit entity)
	{
		writer.WriteStartElement("onexit");

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IOnEntry entity)
	{
		writer.WriteStartElement("onentry");

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IData entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("data");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id);
		}

		if (entity.Source?.Uri is { } source)
		{
			writer.WriteAttributeString(localName: "src", source.ToString());
		}

		if (entity.Expression?.Expression is { } expression)
		{
			writer.WriteAttributeString(localName: "expr", expression);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IInvoke entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("invoke");

		if (entity.Type is { } type)
		{
			writer.WriteAttributeString(localName: "type", type.ToString());
		}

		if (entity.TypeExpression?.Expression is { } typeExpression)
		{
			writer.WriteAttributeString(localName: "typeexpr", typeExpression);
		}

		if (entity.Source is { } source)
		{
			writer.WriteAttributeString(localName: "src", source.ToString());
		}

		if (entity.SourceExpression?.Expression is { } sourceExpression)
		{
			writer.WriteAttributeString(localName: "eventexpr", sourceExpression);
		}

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id);
		}

		if (entity.IdLocation?.Expression is { } idLocation)
		{
			writer.WriteAttributeString(localName: "idlocation", idLocation);
		}

		if (!entity.NameList.IsDefaultOrEmpty)
		{
			writer.WriteStartAttribute("namelist");
			WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
			writer.WriteEndAttribute();
		}

		if (entity.AutoForward)
		{
			writer.WriteAttributeString(localName: "autoforward", value: @"true");
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IFinalize entity)
	{
		writer.WriteStartElement("finalize");

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IParam entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("param");

		if (entity.Name is { } name)
		{
			writer.WriteAttributeString(localName: "name", name);
		}

		if (entity.Expression?.Expression is { } expression)
		{
			writer.WriteAttributeString(localName: "expr", expression);
		}

		if (entity.Location?.Expression is { } location)
		{
			writer.WriteAttributeString(localName: "location", location);
		}

		writer.WriteEndElement();
	}

	protected override void Visit(ref IContent entity)
	{
		Infra.Requires(entity);

		writer.WriteStartElement("content");

		if (entity.Expression?.Expression is { } expression)
		{
			writer.WriteAttributeString(localName: "expr", expression);
		}

		base.Visit(ref entity);

		writer.WriteEndElement();
	}

	protected override void Visit(ref IContentBody entity)
	{
		Infra.Requires(entity);

		if (entity.Value is { } value)
		{
			writer.WriteRaw(value);
		}
	}

	protected override void Visit(ref IInlineContent entity)
	{
		Infra.Requires(entity);

		if (entity.Value is { } value)
		{
			writer.WriteRaw(value);
		}
	}

	protected override void Visit(ref IDoneData entity)
	{
		writer.WriteStartElement("donedata");

		base.Visit(ref entity);

		writer.WriteEndElement();
	}
}