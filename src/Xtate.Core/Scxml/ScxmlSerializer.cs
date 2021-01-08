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
using System.Xml;
using Xtate.Annotations;
using Xtate.Core;

namespace Xtate.Scxml
{
	[PublicAPI]
	public class ScxmlSerializer : StateMachineVisitor
	{
		private const string ScxmlNs      = "http://www.w3.org/2005/07/scxml";
		private const string XtateScxmlNs = "http://xtate.net/scxml";
		private const string Space        = " ";

		private readonly XmlWriter _writer;

		public ScxmlSerializer(XmlWriter writer) => _writer = writer;

		public static void Serialize(IStateMachine stateMachine, XmlWriter writer)
		{
			new ScxmlSerializer(writer).Visit(ref stateMachine);
		}

		protected override void Build(ref IStateMachine entity, ref StateMachineEntity properties)
		{
			_writer.WriteStartElement(prefix: "", localName: "scxml", ScxmlNs);
			_writer.WriteAttributeString(localName: "version", value: @"1.0");

			if (properties.DataModelType is { } dataModelType)
			{
				_writer.WriteAttributeString(localName: "datamodel", dataModelType);
			}

			var target = properties.Initial?.Transition?.Target ?? default;
			if (!target.IsDefaultOrEmpty)
			{
				_writer.WriteStartAttribute("initial");
				WriteArray(target, i => i.Value, Space);
				_writer.WriteEndAttribute();
			}

			if (entity.Is<IStateMachineOptions>(out var options))
			{
				WriteOptions(options);
			}

			properties.Initial = null;
			base.Build(ref entity, ref properties);

			_writer.WriteEndElement();
		}

		private void WriteOptions(IStateMachineOptions options)
		{
			if (options.PersistenceLevel is { } persistenceLevel)
			{
				_writer.WriteAttributeString(localName: "persistence", XtateScxmlNs, persistenceLevel.ToString());
			}

			if (options.SynchronousEventProcessing is { } synchronousEventProcessing)
			{
				_writer.WriteAttributeString(localName: "synchronous", XtateScxmlNs, XmlConvert.ToString(synchronousEventProcessing));
			}

			if (options.ExternalQueueSize is { } externalQueueSize)
			{
				_writer.WriteAttributeString(localName: "queueSize", XtateScxmlNs, XmlConvert.ToString(externalQueueSize));
			}

			if (options.UnhandledErrorBehaviour is { } unhandledErrorBehaviour)
			{
				_writer.WriteAttributeString(localName: "onError", XtateScxmlNs, unhandledErrorBehaviour.ToString());
			}
		}

		protected override void Visit(ref IInitial entity)
		{
			_writer.WriteStartElement("initial");

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IState entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("state");

			if (entity.Id is { } id)
			{
				_writer.WriteAttributeString(localName: "id", id.Value);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IParallel entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("parallel");

			if (entity.Id is { } id)
			{
				_writer.WriteAttributeString(localName: "id", id.Value);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IHistory entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("history");

			if (entity.Id is { } id)
			{
				_writer.WriteAttributeString(localName: "id", id.Value);
			}

			if (entity.Type != HistoryType.Shallow)
			{
				_writer.WriteAttributeString(localName: "type", value: @"deep");
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IFinal entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("final");

			if (entity.Id is { } id)
			{
				_writer.WriteAttributeString(localName: "id", id.Value);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref ITransition entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("transition");

			if (entity.Type != TransitionType.External)
			{
				_writer.WriteAttributeString(localName: "type", value: @"internal");
			}

			if (!entity.EventDescriptors.IsDefaultOrEmpty)
			{
				_writer.WriteStartAttribute("event");
				WriteArray(entity.EventDescriptors, ed => ed.Value, Space);
				_writer.WriteEndAttribute();
			}

			var condition = entity.Condition?.As<IConditionExpression>().Expression;
			if (condition is not null)
			{
				_writer.WriteAttributeString(localName: "cond", condition);
			}

			if (!entity.Target.IsDefaultOrEmpty)
			{
				_writer.WriteStartAttribute("target");
				WriteArray(entity.Target, i => i.Value, Space);
				_writer.WriteEndAttribute();
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		private void WriteArray<T>(ImmutableArray<T> array, Func<T, string> converter, string delimiter)
		{
			var writeDelimiter = false;
			foreach (var item in array)
			{
				if (writeDelimiter)
				{
					_writer.WriteString(delimiter);
				}

				_writer.WriteString(converter(item));

				writeDelimiter = true;
			}
		}

		protected override void Visit(ref ISend entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("send");

			if (entity.EventName is { } eventName)
			{
				_writer.WriteAttributeString(localName: "event", eventName);
			}

			if (entity.EventExpression?.Expression is { } expression)
			{
				_writer.WriteAttributeString(localName: "eventexpr", expression);
			}

			if (entity.Target is { } target)
			{
				_writer.WriteAttributeString(localName: "target", target.ToString());
			}

			if (entity.TargetExpression?.Expression is { } targetExpression)
			{
				_writer.WriteAttributeString(localName: "targetexpr", targetExpression);
			}

			if (entity.Type is { } type)
			{
				_writer.WriteAttributeString(localName: "type", type.ToString());
			}

			if (entity.TypeExpression?.Expression is { } typeExpression)
			{
				_writer.WriteAttributeString(localName: "typeexpr", typeExpression);
			}

			if (entity.Id is { } id)
			{
				_writer.WriteAttributeString(localName: "id", id);
			}

			if (entity.IdLocation?.Expression is { } idLocation)
			{
				_writer.WriteAttributeString(localName: "idlocation", idLocation);
			}

			if (entity.DelayMs is { } ms)
			{
				var delayStr = ms % 1000 == 0 ? ms / 1000 + @"s" : ms + @"ms";
				_writer.WriteAttributeString(localName: "delay", delayStr);
			}

			if (entity.DelayExpression?.Expression is { } delayExpression)
			{
				_writer.WriteAttributeString(localName: "delayexpr", delayExpression);
			}

			if (!entity.NameList.IsDefaultOrEmpty)
			{
				_writer.WriteStartAttribute("namelist");
				WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
				_writer.WriteEndAttribute();
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IScript entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("script");

			if (entity.Source?.Uri is { } source)
			{
				_writer.WriteAttributeString(localName: "src", source.ToString());
			}

			if (entity.Content?.Expression is { } content)
			{
				_writer.WriteRaw(content);
			}

			_writer.WriteEndElement();
		}

		protected override void Build(ref ICustomAction entity, ref CustomActionEntity properties)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			if (entity.Xml is { } xml)
			{
				_writer.WriteRaw(xml);
			}
		}

		protected override void Visit(ref IRaise entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("raise");

			var nameParts = entity.OutgoingEvent?.NameParts ?? default;
			if (!nameParts.IsDefaultOrEmpty)
			{
				_writer.WriteStartAttribute("event");
				EventName.WriteXml(_writer, nameParts);
				_writer.WriteEndAttribute();
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref ILog entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("log");

			if (entity.Label is { } label)
			{
				_writer.WriteAttributeString(localName: "label", label);
			}

			if (entity.Expression?.Expression is { } expression)
			{
				_writer.WriteAttributeString(localName: "expr", expression);
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IIf entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("if");

			if (entity.Condition?.Expression is { } condition)
			{
				_writer.WriteAttributeString(localName: "cond", condition);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IForEach entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("foreach");

			if (entity.Array?.Expression is { } array)
			{
				_writer.WriteAttributeString(localName: "array", array);
			}

			if (entity.Item?.Expression is { } item)
			{
				_writer.WriteAttributeString(localName: "item", item);
			}

			if (entity.Index?.Expression is { } index)
			{
				_writer.WriteAttributeString(localName: "index", index);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IElseIf entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("elseif");

			if (entity.Condition?.Expression is { } condition)
			{
				_writer.WriteAttributeString(localName: "cond", condition);
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IElse entity)
		{
			_writer.WriteStartElement("else");
			_writer.WriteEndElement();
		}

		protected override void Visit(ref ICancel entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("cancel");

			if (entity.SendId is { } sendId)
			{
				_writer.WriteAttributeString(localName: "sendid", sendId);
			}

			if (entity.SendIdExpression?.Expression is { } senIdExpression)
			{
				_writer.WriteAttributeString(localName: "sendidexpr", senIdExpression);
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IAssign entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("assign");

			if (entity.Location?.Expression is { } location)
			{
				_writer.WriteAttributeString(localName: "location", location);
			}

			if (entity.Expression?.Expression is { } expression)
			{
				_writer.WriteAttributeString(localName: "expr", expression);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IDataModel entity)
		{
			_writer.WriteStartElement("datamodel");

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IOnExit entity)
		{
			_writer.WriteStartElement("onexit");

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IOnEntry entity)
		{
			_writer.WriteStartElement("onentry");

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IData entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("data");

			if (entity.Id is { } id)
			{
				_writer.WriteAttributeString(localName: "id", id);
			}

			if (entity.Source?.Uri is { } source)
			{
				_writer.WriteAttributeString(localName: "src", source.ToString());
			}

			if (entity.Expression?.Expression is { } expression)
			{
				_writer.WriteAttributeString(localName: "expr", expression);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IInvoke entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("invoke");

			if (entity.Type is { } type)
			{
				_writer.WriteAttributeString(localName: "type", type.ToString());
			}

			if (entity.TypeExpression?.Expression is { } typeExpression)
			{
				_writer.WriteAttributeString(localName: "typeexpr", typeExpression);
			}

			if (entity.Source is { } source)
			{
				_writer.WriteAttributeString(localName: "src", source.ToString());
			}

			if (entity.SourceExpression?.Expression is { } sourceExpression)
			{
				_writer.WriteAttributeString(localName: "eventexpr", sourceExpression);
			}

			if (entity.Id is { } id)
			{
				_writer.WriteAttributeString(localName: "id", id);
			}

			if (entity.IdLocation?.Expression is { } idLocation)
			{
				_writer.WriteAttributeString(localName: "idlocation", idLocation);
			}

			if (!entity.NameList.IsDefaultOrEmpty)
			{
				_writer.WriteStartAttribute("namelist");
				WriteArray(entity.NameList, le => le.Expression ?? string.Empty, Space);
				_writer.WriteEndAttribute();
			}

			if (entity.AutoForward)
			{
				_writer.WriteAttributeString(localName: "autoforward", value: @"true");
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IFinalize entity)
		{
			_writer.WriteStartElement("finalize");

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IParam entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("param");

			if (entity.Name is { } name)
			{
				_writer.WriteAttributeString(localName: "name", name);
			}

			if (entity.Expression?.Expression is { } expression)
			{
				_writer.WriteAttributeString(localName: "expr", expression);
			}

			if (entity.Location?.Expression is { } location)
			{
				_writer.WriteAttributeString(localName: "location", location);
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IContent entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("content");

			if (entity.Expression?.Expression is { } expression)
			{
				_writer.WriteAttributeString(localName: "expr", expression);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IContentBody entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			if (entity.Value is { } value)
			{
				_writer.WriteRaw(value);
			}
		}

		protected override void Visit(ref IInlineContent entity)
		{
			if (entity is null) throw new ArgumentNullException(nameof(entity));

			if (entity.Value is { } value)
			{
				_writer.WriteRaw(value);
			}
		}

		protected override void Visit(ref IDoneData entity)
		{
			_writer.WriteStartElement("donedata");

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}
	}
}