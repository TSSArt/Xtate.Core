#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Collections.Immutable;
using System.Xml;
using Xtate.Annotations;

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

			if (properties.DataModelType != null)
			{
				_writer.WriteAttributeString(localName: "datamodel", properties.DataModelType);
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
			if (options.PersistenceLevel != null)
			{
				_writer.WriteAttributeString(localName: "persistence", XtateScxmlNs, options.PersistenceLevel.Value.ToString());
			}

			if (options.SynchronousEventProcessing != null)
			{
				_writer.WriteAttributeString(localName: "synchronous", XtateScxmlNs, XmlConvert.ToString(options.SynchronousEventProcessing.Value));
			}

			if (options.ExternalQueueSize != null)
			{
				_writer.WriteAttributeString(localName: "queueSize", XtateScxmlNs, XmlConvert.ToString(options.ExternalQueueSize.Value));
			}

			if (options.UnhandledErrorBehaviour != null)
			{
				_writer.WriteAttributeString(localName: "onError", XtateScxmlNs, options.UnhandledErrorBehaviour.Value.ToString());
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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("state");

			if (entity.Id != null)
			{
				_writer.WriteAttributeString(localName: "id", entity.Id.Value);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IParallel entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("parallel");

			if (entity.Id != null)
			{
				_writer.WriteAttributeString(localName: "id", entity.Id.Value);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IHistory entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("history");

			if (entity.Id != null)
			{
				_writer.WriteAttributeString(localName: "id", entity.Id.Value);
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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("final");

			if (entity.Id != null)
			{
				_writer.WriteAttributeString(localName: "id", entity.Id.Value);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref ITransition entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

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
			if (condition != null)
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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("send");

			if (entity.EventName != null)
			{
				_writer.WriteAttributeString(localName: "event", entity.EventName);
			}

			if (entity.EventExpression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "eventexpr", entity.EventExpression.Expression);
			}

			if (entity.Target != null)
			{
				_writer.WriteAttributeString(localName: "target", entity.Target.ToString());
			}

			if (entity.TargetExpression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "targetexpr", entity.TargetExpression.Expression);
			}

			if (entity.Type != null)
			{
				_writer.WriteAttributeString(localName: "type", entity.Type.ToString());
			}

			if (entity.TypeExpression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "typeexpr", entity.TypeExpression.Expression);
			}

			if (entity.Id != null)
			{
				_writer.WriteAttributeString(localName: "id", entity.Id);
			}

			if (entity.IdLocation?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "idlocation", entity.IdLocation.Expression);
			}

			if (entity.DelayMs != null)
			{
				var ms = entity.DelayMs.Value;
				var delayStr = ms % 1000 == 0 ? ms / 1000 + @"s" : ms + @"ms";
				_writer.WriteAttributeString(localName: "delay", delayStr);
			}

			if (entity.DelayExpression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "delayexpr", entity.DelayExpression.Expression);
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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("script");

			if (entity.Source?.Uri != null)
			{
				_writer.WriteAttributeString(localName: "src", entity.Source.Uri.ToString());
			}

			if (entity.Content?.Expression != null)
			{
				_writer.WriteRaw(entity.Content.Expression);
			}

			_writer.WriteEndElement();
		}

		protected override void Build(ref ICustomAction entity, ref CustomActionEntity properties)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (entity.Xml != null)
			{
				_writer.WriteRaw(entity.Xml);
			}
		}

		protected override void Visit(ref IRaise entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("log");

			if (entity.Label != null)
			{
				_writer.WriteAttributeString(localName: "label", entity.Label);
			}

			if (entity.Expression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "expr", entity.Expression.Expression);
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IIf entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("if");

			if (entity.Condition?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "cond", entity.Condition.Expression);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IForEach entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("foreach");

			if (entity.Array?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "array", entity.Array.Expression);
			}

			if (entity.Item?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "item", entity.Item.Expression);
			}

			if (entity.Index?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "index", entity.Index.Expression);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IElseIf entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("elseif");

			if (entity.Condition?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "cond", entity.Condition.Expression);
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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("cancel");

			if (entity.SendId != null)
			{
				_writer.WriteAttributeString(localName: "index", entity.SendId);
			}

			if (entity.SendIdExpression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "index", entity.SendIdExpression.Expression);
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IAssign entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("assign");

			if (entity.Location?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "location", entity.Location.Expression);
			}

			if (entity.Expression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "expr", entity.Expression.Expression);
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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("data");

			if (entity.Id != null)
			{
				_writer.WriteAttributeString(localName: "id", entity.Id);
			}

			if (entity.Source?.Uri != null)
			{
				_writer.WriteAttributeString(localName: "src", entity.Source.Uri.ToString());
			}

			if (entity.Expression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "expr", entity.Expression.Expression);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IInvoke entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("invoke");

			if (entity.Type != null)
			{
				_writer.WriteAttributeString(localName: "type", entity.Type.ToString());
			}

			if (entity.TypeExpression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "typeexpr", entity.TypeExpression.Expression);
			}

			if (entity.Source != null)
			{
				_writer.WriteAttributeString(localName: "src", entity.Source.ToString());
			}

			if (entity.SourceExpression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "eventexpr", entity.SourceExpression.Expression);
			}

			if (entity.Id != null)
			{
				_writer.WriteAttributeString(localName: "id", entity.Id);
			}

			if (entity.IdLocation?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "idlocation", entity.IdLocation.Expression);
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
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("param");

			if (entity.Name != null)
			{
				_writer.WriteAttributeString(localName: "name", entity.Name);
			}

			if (entity.Expression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "expr", entity.Expression.Expression);
			}

			if (entity.Location?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "location", entity.Location.Expression);
			}

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IContent entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			_writer.WriteStartElement("content");

			if (entity.Expression?.Expression != null)
			{
				_writer.WriteAttributeString(localName: "expr", entity.Expression.Expression);
			}

			base.Visit(ref entity);

			_writer.WriteEndElement();
		}

		protected override void Visit(ref IContentBody entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (entity.Value != null)
			{
				_writer.WriteRaw(entity.Value);
			}
		}

		protected override void Visit(ref IInlineContent entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (entity.Value != null)
			{
				_writer.WriteRaw(entity.Value);
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