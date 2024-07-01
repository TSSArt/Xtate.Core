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
using System.Xml;
using Xtate.Core;

namespace Xtate.Scxml;

public class ScxmlSerializerWriter : StateMachineVisitor
=======
using System.Xml;

namespace Xtate.Scxml;

public class ScxmlSerializerWriter(XmlWriter writer) : StateMachineVisitor
>>>>>>> Stashed changes
{
	private const string ScxmlNs      = "http://www.w3.org/2005/07/scxml";
	private const string XtateScxmlNs = "http://xtate.net/scxml";
	private const string Space        = " ";

<<<<<<< Updated upstream
	private readonly XmlWriter _writer;

	public ScxmlSerializerWriter(XmlWriter writer) => _writer = writer;

=======
>>>>>>> Stashed changes
	public void Serialize(IStateMachine stateMachine) => Visit(ref stateMachine);

	protected override void Visit(ref IStateMachine entity)
	{
		Infra.NotNull(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement(prefix: "", localName: "scxml", ScxmlNs);
		_writer.WriteAttributeString(localName: "version", value: @"1.0");

		if (entity.DataModelType is { } dataModelType)
		{
			_writer.WriteAttributeString(localName: "datamodel", dataModelType);
=======
		writer.WriteStartElement(prefix: "", localName: "scxml", ScxmlNs);
		writer.WriteAttributeString(localName: "version", value: @"1.0");

		if (entity.DataModelType is { } dataModelType)
		{
			writer.WriteAttributeString(localName: "datamodel", dataModelType);
>>>>>>> Stashed changes
		}

		var target = entity.Initial?.Transition?.Target ?? default;
		if (!target.IsDefaultOrEmpty)
		{
<<<<<<< Updated upstream
			_writer.WriteStartAttribute("initial");
			WriteArray(target, i => i.Value, Space);
			_writer.WriteEndAttribute();
=======
			writer.WriteStartAttribute("initial");
			WriteArray(target, i => i.Value, Space);
			writer.WriteEndAttribute();
>>>>>>> Stashed changes
		}

		if (entity.Is<IStateMachineOptions>(out var options))
		{
			WriteOptions(options);
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "persistence", XtateScxmlNs, persistenceLevel.ToString());
=======
			writer.WriteAttributeString(localName: "persistence", XtateScxmlNs, persistenceLevel.ToString());
>>>>>>> Stashed changes
		}

		if (options.SynchronousEventProcessing is { } synchronousEventProcessing)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "synchronous", XtateScxmlNs, XmlConvert.ToString(synchronousEventProcessing));
=======
			writer.WriteAttributeString(localName: "synchronous", XtateScxmlNs, XmlConvert.ToString(synchronousEventProcessing));
>>>>>>> Stashed changes
		}

		if (options.ExternalQueueSize is { } externalQueueSize)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "queueSize", XtateScxmlNs, XmlConvert.ToString(externalQueueSize));
=======
			writer.WriteAttributeString(localName: "queueSize", XtateScxmlNs, XmlConvert.ToString(externalQueueSize));
>>>>>>> Stashed changes
		}

		if (options.UnhandledErrorBehaviour is { } unhandledErrorBehaviour)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "onError", XtateScxmlNs, unhandledErrorBehaviour.ToString());
=======
			writer.WriteAttributeString(localName: "onError", XtateScxmlNs, unhandledErrorBehaviour.ToString());
>>>>>>> Stashed changes
		}
	}

	protected override void Visit(ref IInitial entity)
	{
<<<<<<< Updated upstream
		_writer.WriteStartElement("initial");

		base.Visit(ref entity);

		_writer.WriteEndElement();
=======
		writer.WriteStartElement("initial");

		base.Visit(ref entity);

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IState entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("state");

		if (entity.Id is { } id)
		{
			_writer.WriteAttributeString(localName: "id", id.Value);
=======
		writer.WriteStartElement("state");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IParallel entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("parallel");

		if (entity.Id is { } id)
		{
			_writer.WriteAttributeString(localName: "id", id.Value);
=======
		writer.WriteStartElement("parallel");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IHistory entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("history");

		if (entity.Id is { } id)
		{
			_writer.WriteAttributeString(localName: "id", id.Value);
=======
		writer.WriteStartElement("history");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
>>>>>>> Stashed changes
		}

		if (entity.Type != HistoryType.Shallow)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "type", value: @"deep");
=======
			writer.WriteAttributeString(localName: "type", value: @"deep");
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IFinal entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("final");

		if (entity.Id is { } id)
		{
			_writer.WriteAttributeString(localName: "id", id.Value);
=======
		writer.WriteStartElement("final");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id.Value);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref ITransition entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("transition");

		if (entity.Type != TransitionType.External)
		{
			_writer.WriteAttributeString(localName: "type", value: @"internal");
=======
		writer.WriteStartElement("transition");

		if (entity.Type != TransitionType.External)
		{
			writer.WriteAttributeString(localName: "type", value: @"internal");
>>>>>>> Stashed changes
		}

		if (!entity.EventDescriptors.IsDefaultOrEmpty)
		{
<<<<<<< Updated upstream
			_writer.WriteStartAttribute("event");
			WriteArray(entity.EventDescriptors, ed => ed.Value, Space);
			_writer.WriteEndAttribute();
=======
			writer.WriteStartAttribute("event");
			WriteArray(entity.EventDescriptors, ed => ed.Value, Space);
			writer.WriteEndAttribute();
>>>>>>> Stashed changes
		}

		var condition = entity.Condition?.As<IConditionExpression>().Expression;
		if (condition is not null)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "cond", condition);
=======
			writer.WriteAttributeString(localName: "cond", condition);
>>>>>>> Stashed changes
		}

		if (!entity.Target.IsDefaultOrEmpty)
		{
<<<<<<< Updated upstream
			_writer.WriteStartAttribute("target");
			WriteArray(entity.Target, i => i.Value, Space);
			_writer.WriteEndAttribute();
=======
			writer.WriteStartAttribute("target");
			WriteArray(entity.Target, i => i.Value, Space);
			writer.WriteEndAttribute();
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	private void WriteArray<T>(ImmutableArray<T> array, Func<T, string> converter, string delimiter)
	{
		var writeDelimiter = false;
		foreach (var item in array)
		{
			if (writeDelimiter)
			{
<<<<<<< Updated upstream
				_writer.WriteString(delimiter);
			}

			_writer.WriteString(converter(item));
=======
				writer.WriteString(delimiter);
			}

			writer.WriteString(converter(item));
>>>>>>> Stashed changes

			writeDelimiter = true;
		}
	}

	protected override void Visit(ref ISend entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("send");

		if (entity.EventName is { } eventName)
		{
			_writer.WriteAttributeString(localName: "event", eventName);
=======
		writer.WriteStartElement("send");

		if (entity.EventName is { } eventName)
		{
			writer.WriteAttributeString(localName: "event", eventName);
>>>>>>> Stashed changes
		}

		if (entity.EventExpression?.Expression is { } expression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "eventexpr", expression);
=======
			writer.WriteAttributeString(localName: "eventexpr", expression);
>>>>>>> Stashed changes
		}

		if (entity.Target is { } target)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "target", target.ToString());
=======
			writer.WriteAttributeString(localName: "target", target.ToString());
>>>>>>> Stashed changes
		}

		if (entity.TargetExpression?.Expression is { } targetExpression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "targetexpr", targetExpression);
=======
			writer.WriteAttributeString(localName: "targetexpr", targetExpression);
>>>>>>> Stashed changes
		}

		if (entity.Type is { } type)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "type", type.ToString());
=======
			writer.WriteAttributeString(localName: "type", type.ToString());
>>>>>>> Stashed changes
		}

		if (entity.TypeExpression?.Expression is { } typeExpression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "typeexpr", typeExpression);
=======
			writer.WriteAttributeString(localName: "typeexpr", typeExpression);
>>>>>>> Stashed changes
		}

		if (entity.Id is { } id)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "id", id);
=======
			writer.WriteAttributeString(localName: "id", id);
>>>>>>> Stashed changes
		}

		if (entity.IdLocation?.Expression is { } idLocation)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "idlocation", idLocation);
=======
			writer.WriteAttributeString(localName: "idlocation", idLocation);
>>>>>>> Stashed changes
		}

		if (entity.DelayMs is { } ms)
		{
			var delayStr = ms % 1000 == 0 ? ms / 1000 + @"s" : ms + @"ms";
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "delay", delayStr);
=======
			writer.WriteAttributeString(localName: "delay", delayStr);
>>>>>>> Stashed changes
		}

		if (entity.DelayExpression?.Expression is { } delayExpression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "delayexpr", delayExpression);
=======
			writer.WriteAttributeString(localName: "delayexpr", delayExpression);
>>>>>>> Stashed changes
		}

		if (!entity.NameList.IsDefaultOrEmpty)
		{
<<<<<<< Updated upstream
			_writer.WriteStartAttribute("namelist");
			WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
			_writer.WriteEndAttribute();
=======
			writer.WriteStartAttribute("namelist");
			WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
			writer.WriteEndAttribute();
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IScript entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("script");

		if (entity.Source?.Uri is { } source)
		{
			_writer.WriteAttributeString(localName: "src", source.ToString());
=======
		writer.WriteStartElement("script");

		if (entity.Source?.Uri is { } source)
		{
			writer.WriteAttributeString(localName: "src", source.ToString());
>>>>>>> Stashed changes
		}

		if (entity.Content?.Expression is { } content)
		{
<<<<<<< Updated upstream
			_writer.WriteRaw(content);
		}

		_writer.WriteEndElement();
=======
			writer.WriteRaw(content);
		}

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref ICustomAction entity)
	{
		Infra.Requires(entity);

		if (entity.Xml is { } xml)
		{
<<<<<<< Updated upstream
			_writer.WriteRaw(xml);
=======
			writer.WriteRaw(xml);
>>>>>>> Stashed changes
		}
	}

	protected override void Visit(ref IRaise entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("raise");
=======
		writer.WriteStartElement("raise");
>>>>>>> Stashed changes

		var nameParts = entity.OutgoingEvent?.NameParts ?? default;
		if (!nameParts.IsDefaultOrEmpty)
		{
<<<<<<< Updated upstream
			_writer.WriteStartAttribute("event");
			EventName.WriteXml(_writer, nameParts);
			_writer.WriteEndAttribute();
		}

		_writer.WriteEndElement();
=======
			writer.WriteStartAttribute("event");
			EventName.WriteXml(writer, nameParts);
			writer.WriteEndAttribute();
		}

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref ILog entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("log");

		if (entity.Label is { } label)
		{
			_writer.WriteAttributeString(localName: "label", label);
=======
		writer.WriteStartElement("log");

		if (entity.Label is { } label)
		{
			writer.WriteAttributeString(localName: "label", label);
>>>>>>> Stashed changes
		}

		if (entity.Expression?.Expression is { } expression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "expr", expression);
		}

		_writer.WriteEndElement();
=======
			writer.WriteAttributeString(localName: "expr", expression);
		}

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IIf entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("if");

		if (entity.Condition?.Expression is { } condition)
		{
			_writer.WriteAttributeString(localName: "cond", condition);
=======
		writer.WriteStartElement("if");

		if (entity.Condition?.Expression is { } condition)
		{
			writer.WriteAttributeString(localName: "cond", condition);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IForEach entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("foreach");

		if (entity.Array?.Expression is { } array)
		{
			_writer.WriteAttributeString(localName: "array", array);
=======
		writer.WriteStartElement("foreach");

		if (entity.Array?.Expression is { } array)
		{
			writer.WriteAttributeString(localName: "array", array);
>>>>>>> Stashed changes
		}

		if (entity.Item?.Expression is { } item)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "item", item);
=======
			writer.WriteAttributeString(localName: "item", item);
>>>>>>> Stashed changes
		}

		if (entity.Index?.Expression is { } index)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "index", index);
=======
			writer.WriteAttributeString(localName: "index", index);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IElseIf entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("elseif");

		if (entity.Condition?.Expression is { } condition)
		{
			_writer.WriteAttributeString(localName: "cond", condition);
		}

		_writer.WriteEndElement();
=======
		writer.WriteStartElement("elseif");

		if (entity.Condition?.Expression is { } condition)
		{
			writer.WriteAttributeString(localName: "cond", condition);
		}

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IElse entity)
	{
<<<<<<< Updated upstream
		_writer.WriteStartElement("else");
		_writer.WriteEndElement();
=======
		writer.WriteStartElement("else");
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref ICancel entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("cancel");

		if (entity.SendId is { } sendId)
		{
			_writer.WriteAttributeString(localName: "sendid", sendId);
=======
		writer.WriteStartElement("cancel");

		if (entity.SendId is { } sendId)
		{
			writer.WriteAttributeString(localName: "sendid", sendId);
>>>>>>> Stashed changes
		}

		if (entity.SendIdExpression?.Expression is { } senIdExpression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "sendidexpr", senIdExpression);
		}

		_writer.WriteEndElement();
=======
			writer.WriteAttributeString(localName: "sendidexpr", senIdExpression);
		}

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IAssign entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("assign");

		if (entity.Location?.Expression is { } location)
		{
			_writer.WriteAttributeString(localName: "location", location);
=======
		writer.WriteStartElement("assign");

		if (entity.Location?.Expression is { } location)
		{
			writer.WriteAttributeString(localName: "location", location);
>>>>>>> Stashed changes
		}

		if (entity.Expression?.Expression is { } expression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "expr", expression);
=======
			writer.WriteAttributeString(localName: "expr", expression);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IDataModel entity)
	{
<<<<<<< Updated upstream
		_writer.WriteStartElement("datamodel");

		base.Visit(ref entity);

		_writer.WriteEndElement();
=======
		writer.WriteStartElement("datamodel");

		base.Visit(ref entity);

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IOnExit entity)
	{
<<<<<<< Updated upstream
		_writer.WriteStartElement("onexit");

		base.Visit(ref entity);

		_writer.WriteEndElement();
=======
		writer.WriteStartElement("onexit");

		base.Visit(ref entity);

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IOnEntry entity)
	{
<<<<<<< Updated upstream
		_writer.WriteStartElement("onentry");

		base.Visit(ref entity);

		_writer.WriteEndElement();
=======
		writer.WriteStartElement("onentry");

		base.Visit(ref entity);

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IData entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("data");

		if (entity.Id is { } id)
		{
			_writer.WriteAttributeString(localName: "id", id);
=======
		writer.WriteStartElement("data");

		if (entity.Id is { } id)
		{
			writer.WriteAttributeString(localName: "id", id);
>>>>>>> Stashed changes
		}

		if (entity.Source?.Uri is { } source)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "src", source.ToString());
=======
			writer.WriteAttributeString(localName: "src", source.ToString());
>>>>>>> Stashed changes
		}

		if (entity.Expression?.Expression is { } expression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "expr", expression);
=======
			writer.WriteAttributeString(localName: "expr", expression);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IInvoke entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("invoke");

		if (entity.Type is { } type)
		{
			_writer.WriteAttributeString(localName: "type", type.ToString());
=======
		writer.WriteStartElement("invoke");

		if (entity.Type is { } type)
		{
			writer.WriteAttributeString(localName: "type", type.ToString());
>>>>>>> Stashed changes
		}

		if (entity.TypeExpression?.Expression is { } typeExpression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "typeexpr", typeExpression);
=======
			writer.WriteAttributeString(localName: "typeexpr", typeExpression);
>>>>>>> Stashed changes
		}

		if (entity.Source is { } source)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "src", source.ToString());
=======
			writer.WriteAttributeString(localName: "src", source.ToString());
>>>>>>> Stashed changes
		}

		if (entity.SourceExpression?.Expression is { } sourceExpression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "eventexpr", sourceExpression);
=======
			writer.WriteAttributeString(localName: "eventexpr", sourceExpression);
>>>>>>> Stashed changes
		}

		if (entity.Id is { } id)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "id", id);
=======
			writer.WriteAttributeString(localName: "id", id);
>>>>>>> Stashed changes
		}

		if (entity.IdLocation?.Expression is { } idLocation)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "idlocation", idLocation);
=======
			writer.WriteAttributeString(localName: "idlocation", idLocation);
>>>>>>> Stashed changes
		}

		if (!entity.NameList.IsDefaultOrEmpty)
		{
<<<<<<< Updated upstream
			_writer.WriteStartAttribute("namelist");
			WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
			_writer.WriteEndAttribute();
=======
			writer.WriteStartAttribute("namelist");
			WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
			writer.WriteEndAttribute();
>>>>>>> Stashed changes
		}

		if (entity.AutoForward)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "autoforward", value: @"true");
=======
			writer.WriteAttributeString(localName: "autoforward", value: @"true");
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IFinalize entity)
	{
<<<<<<< Updated upstream
		_writer.WriteStartElement("finalize");

		base.Visit(ref entity);

		_writer.WriteEndElement();
=======
		writer.WriteStartElement("finalize");

		base.Visit(ref entity);

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IParam entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("param");

		if (entity.Name is { } name)
		{
			_writer.WriteAttributeString(localName: "name", name);
=======
		writer.WriteStartElement("param");

		if (entity.Name is { } name)
		{
			writer.WriteAttributeString(localName: "name", name);
>>>>>>> Stashed changes
		}

		if (entity.Expression?.Expression is { } expression)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "expr", expression);
=======
			writer.WriteAttributeString(localName: "expr", expression);
>>>>>>> Stashed changes
		}

		if (entity.Location?.Expression is { } location)
		{
<<<<<<< Updated upstream
			_writer.WriteAttributeString(localName: "location", location);
		}

		_writer.WriteEndElement();
=======
			writer.WriteAttributeString(localName: "location", location);
		}

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IContent entity)
	{
		Infra.Requires(entity);

<<<<<<< Updated upstream
		_writer.WriteStartElement("content");

		if (entity.Expression?.Expression is { } expression)
		{
			_writer.WriteAttributeString(localName: "expr", expression);
=======
		writer.WriteStartElement("content");

		if (entity.Expression?.Expression is { } expression)
		{
			writer.WriteAttributeString(localName: "expr", expression);
>>>>>>> Stashed changes
		}

		base.Visit(ref entity);

<<<<<<< Updated upstream
		_writer.WriteEndElement();
=======
		writer.WriteEndElement();
>>>>>>> Stashed changes
	}

	protected override void Visit(ref IContentBody entity)
	{
		Infra.Requires(entity);

		if (entity.Value is { } value)
		{
<<<<<<< Updated upstream
			_writer.WriteRaw(value);
=======
			writer.WriteRaw(value);
>>>>>>> Stashed changes
		}
	}

	protected override void Visit(ref IInlineContent entity)
	{
		Infra.Requires(entity);

		if (entity.Value is { } value)
		{
<<<<<<< Updated upstream
			_writer.WriteRaw(value);
=======
			writer.WriteRaw(value);
>>>>>>> Stashed changes
		}
	}

	protected override void Visit(ref IDoneData entity)
	{
<<<<<<< Updated upstream
		_writer.WriteStartElement("donedata");

		base.Visit(ref entity);

		_writer.WriteEndElement();
=======
		writer.WriteStartElement("donedata");

		base.Visit(ref entity);

		writer.WriteEndElement();
>>>>>>> Stashed changes
	}
}