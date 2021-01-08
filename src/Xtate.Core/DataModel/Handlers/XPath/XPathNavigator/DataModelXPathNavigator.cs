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
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using Xtate.Core;

namespace Xtate.DataModel.XPath
{
	public class DataModelXPathNavigator : XPathNavigator
	{
		private const int PathFieldCount = 6;

		private Node   _path0;
		private Node   _path1;
		private Node   _path2;
		private Node   _path3;
		private Node   _path4;
		private Node   _path5;
		private int    _pathLength;
		private Node[] _pathOther = Array.Empty<Node>();

		public DataModelXPathNavigator(in DataModelValue root)
		{
			NameTable = new NameTable();

			PathItem(index: 0) = new Node(root, AdapterFactory.GetDefaultAdapter(root));
		}

		private DataModelXPathNavigator(DataModelXPathNavigator source)
		{
			NameTable = source.NameTable;

			ClonePosition(source, this);
		}

		private ref Node Parent  => ref PathItem(_pathLength - 1);
		private ref Node Current => ref PathItem(_pathLength);

		public override XPathNodeType NodeType       => Current.GetNodeType();
		public override string        Value          => Current.GetValue();
		public override string        Name           => Current.GetName();
		public override string        Prefix         => Current.GetPrefix();
		public override string        BaseURI        => string.Empty;
		public override string        NamespaceURI   => Current.GetNamespaceUri();
		public override string        LocalName      => Current.GetLocalName();
		public override bool          IsEmptyElement => Current.IsEmptyElement();

		public override XmlNameTable NameTable { get; }

		public DataModelValue DataModelValue => Current.DataModelValue;

		public DataModelList? Metadata => Current.Metadata;

		public override bool HasAttributes => Current.GetFirstAttribute(out _);

		public override bool HasChildren => Current.GetFirstChild(out _);

		private static void ClonePosition(DataModelXPathNavigator source, DataModelXPathNavigator destination)
		{
			var otherCount = source._pathLength - PathFieldCount + 1;

			if (otherCount < 0)
			{
				otherCount = 0;
			}

			var extraLength = destination._pathOther.Length - otherCount;
			if (extraLength >= 0)
			{
				Array.Clear(destination._pathOther, otherCount, extraLength);
			}
			else
			{
				destination._pathOther = new Node[source._pathOther.Length];
			}

			Array.Copy(source._pathOther, destination._pathOther, otherCount);

			for (var i = 0; i < PathFieldCount; i ++)
			{
				destination.PathItem(i) = source.PathItem(i);
			}

			destination._pathLength = source._pathLength;
		}

		private ref Node PathItem(int index)
		{
			switch (index)
			{
				case 0: return ref _path0;
				case 1: return ref _path1;
				case 2: return ref _path2;
				case 3: return ref _path3;
				case 4: return ref _path4;
				case 5: return ref _path5;
				default: return ref _pathOther[index - PathFieldCount];
			}
		}

		public override bool MoveToId(string id) => false;

		public override bool MoveToFirstChild()
		{
			if (Current.GetFirstChild(out var childNode))
			{
				PushNode(childNode);

				return true;
			}

			return false;
		}

		public override bool MoveToParent()
		{
			if (_pathLength == 0)
			{
				return false;
			}

			PopNode();

			return true;
		}

		public override bool MoveToNext() => _pathLength != 0 && Parent.GetNextChild(ref Current);

		public override bool MoveToPrevious() => _pathLength != 0 && Parent.GetPreviousChild(ref Current);

		public override bool MoveToFirstAttribute()
		{
			if (Current.GetFirstAttribute(out var attributeNode))
			{
				PushNode(attributeNode);

				return true;
			}

			return false;
		}

		public override bool MoveToNextAttribute() => _pathLength != 0 && Parent.GetNextAttribute(ref Current);

		public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
		{
			if (Current.GetFirstNamespace(out var namespaceNode))
			{
				PushNode(namespaceNode);

				return true;
			}

			return MoveToParentFirstNamespace(namespaceScope);
		}

		public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) => _pathLength != 0 && (Parent.GetNextNamespace(ref Current) || MoveToParentFirstNamespace(namespaceScope));

		private bool MoveToParentFirstNamespace(XPathNamespaceScope namespaceScope)
		{
			if (namespaceScope == XPathNamespaceScope.Local)
			{
				return false;
			}

			if (MoveToParent() && MoveToFirstNamespace(namespaceScope))
			{
				return true;
			}

			if (namespaceScope == XPathNamespaceScope.ExcludeXml)
			{
				return false;
			}

			Current = new Node(value: default, AdapterFactory.XmlnsXmlNodeAdapter);

			return true;
		}

		public override XPathNavigator Clone() => new DataModelXPathNavigator(this);

		public override bool IsSamePosition(XPathNavigator other)
		{
			if (other is not DataModelXPathNavigator navigator)
			{
				return false;
			}

			if (_pathLength != navigator._pathLength)
			{
				return false;
			}

			for (var i = 0; i <= _pathLength; i ++)
			{
				if (!PathItem(i).Equals(navigator.PathItem(i)))
				{
					return false;
				}
			}

			return true;
		}

		public override bool MoveTo(XPathNavigator other)
		{
			if (other is not DataModelXPathNavigator navigator)
			{
				return false;
			}

			ClonePosition(navigator, this);

			return true;
		}

		private void PushNode(in Node node)
		{
			_pathLength ++;

			if (_pathLength >= PathFieldCount)
			{
				if (_pathOther.Length == 0)
				{
					_pathOther = new Node[4];
				}
				else if (_pathLength - PathFieldCount == _pathOther.Length)
				{
					var newPathOther = new Node[_pathOther.Length * 2];
					Array.Copy(_pathOther, newPathOther, _pathOther.Length);
					_pathOther = newPathOther;
				}
			}

			PathItem(_pathLength) = node;
		}

		private void PopNode() => PathItem(_pathLength --) = default;

		internal void FirstChild(IObject valueObject) => AddChildren(valueObject, last: false, clear: false);

		internal void LastChild(IObject valueObject) => AddChildren(valueObject, last: true, clear: false);

		internal void ReplaceChildren(IObject valueObject) => AddChildren(valueObject, last: false, clear: true);

		internal void PreviousSibling(IObject valueObject) => AddSiblings(valueObject, offset: 0, replace: false);

		internal void NextSibling(IObject valueObject) => AddSiblings(valueObject, offset: 1, replace: false);

		internal void Replace(IObject valueObject) => AddSiblings(valueObject, offset: 0, replace: true);

		private void AddSiblings(IObject valueObject, int offset, bool replace)
		{
			if (valueObject is null) throw new ArgumentNullException(nameof(valueObject));

			Infrastructure.Assert(NodeType == XPathNodeType.Element);

			if (_pathLength == 0)
			{
				return;
			}

			var list = Parent.DataModelValue.AsList();

			if (ShouldNormalize(Entries(list, replace ? Current.ParentIndex : -1, includeElement: default, valueObject), out var value))
			{
				PopNode();

				SetNewValue(value);
			}
			else
			{
				if (replace)
				{
					list.Remove(Current.ParentIndex);
				}

				AddRange(list, valueObject, Current.ParentIndex + offset);
			}
		}

		private void AddChildren(IObject valueObject, bool last, bool clear)
		{
			if (valueObject is null) throw new ArgumentNullException(nameof(valueObject));

			Infrastructure.Assert(NodeType == XPathNodeType.Element);

			if (_pathLength == 0)
			{
				if (Current.DataModelValue.AsListOrDefault() is { } rootList)
				{
					if (clear)
					{
						rootList.Clear();
					}

					AddRange(rootList, valueObject, last ? rootList.Count : 0);
				}

				return;
			}

			if (Current.DataModelValue.AsListOrDefault() is { } list)
			{
				if (ShouldNormalize(Entries(clear ? DataModelList.Empty : list, excludeIndex: -1, includeElement: default, valueObject), out var value1))
				{
					SetNewValue(value1);
				}
				else
				{
					if (clear)
					{
						list.Clear();
					}

					AddRange(list, valueObject, last ? list.Count : 0);
				}

				return;
			}

			var includeElement = !clear ? new Element(key: default, Current.DataModelValue) : default;

			if (ShouldNormalize(Entries(DataModelList.Empty, excludeIndex: -1, includeElement, valueObject), out var value2))
			{
				SetNewValue(value2);
			}
			else
			{
				list = new DataModelList();

				if (!clear && !Current.DataModelValue.IsUndefined())
				{
					list.Add(key: default, Current.DataModelValue, metadata: default);
				}

				AddRange(list, valueObject, last ? list.Count : 0);

				Parent.DataModelValue.AsList().Set(Current.ParentIndex, Current.ParentProperty, list, metadata: default);
			}
		}

		private static IEnumerable<Element> Entries(DataModelList list, int excludeIndex, Element includeElement, IObject? valueObject)
		{
			foreach (var entry in list.Entries)
			{
				if (entry.Index != excludeIndex)
				{
					yield return new Element(entry.Key, entry.Value);
				}
			}

			if (!includeElement.Value.IsUndefined())
			{
				yield return includeElement;
			}

			if (valueObject is null)
			{
				yield break;
			}

			if (valueObject is XPathObject { Type: XPathObjectType.NodeSet } xPathObject)
			{
				foreach (DataModelXPathNavigator navigator in xPathObject.AsIterator())
				{
					var key = navigator.NodeType == XPathNodeType.Element ? navigator.LocalName : default;

					yield return new Element(key, navigator.Current.DataModelValue);
				}
			}
			else
			{
				yield return new Element(key: default, DataModelValue.FromObject(valueObject));
			}
		}

		private static void AddRange(DataModelList list, IObject valueObject, int start)
		{
			if (valueObject is XPathObject { Type: XPathObjectType.NodeSet } xPathObject)
			{
				foreach (DataModelXPathNavigator navigator in xPathObject.AsIterator())
				{
					var key = XmlConverter.NsNameToKey(navigator.NamespaceURI, navigator.LocalName);
					var metadata = new DataModelValue(navigator.Current.Metadata).CloneAsWritable().AsListOrDefault();
					list.Insert(start ++, key, navigator.Current.DataModelValue.CloneAsWritable(), metadata);
				}

				return;
			}

			var value = DataModelValue.FromObject(valueObject);

			if (value.AsListOrDefault() is { } arr)
			{
				foreach (var entry in arr.Entries)
				{
					list.Insert(start ++, entry.Key, entry.Value.CloneAsWritable(), entry.Metadata?.CloneAsWritable());
				}
			}
			else
			{
				list.Insert(start, key: default, value, metadata: default);
			}
		}

		public override void DeleteSelf()
		{
			if (_pathLength == 0)
			{
				return;
			}

			var list = Parent.DataModelValue.AsList();

			if (ShouldNormalize(Entries(list, Current.ParentIndex, includeElement: default, valueObject: default), out var value))
			{
				PopNode();

				SetNewValue(value);
			}
			else
			{
				list.Remove(Current.ParentIndex);
			}
		}

		private static bool ShouldNormalize(IEnumerable<Element> enumerable, out DataModelValue value)
		{
			using var enumerator = enumerable.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				value = default;

				return true;
			}

			var element = enumerator.Current;

			if (element.Key is null && element.Value.Type != DataModelValueType.List && !enumerator.MoveNext())
			{
				value = element.Value;

				return true;
			}

			value = default;

			return false;
		}

		private void SetNewValue(in DataModelValue value)
		{
			if (_pathLength == 0)
			{
				return;
			}

			var list = Parent.DataModelValue.AsList();

			var includeElement = new Element(Current.ParentProperty, value);
			if (ShouldNormalize(Entries(list, Current.ParentIndex, includeElement, valueObject: default), out var value1))
			{
				PopNode();

				SetNewValue(value1);
			}
			else
			{
				list.Set(Current.ParentIndex, Current.ParentProperty, value, Current.Metadata);
			}
		}

		public override void CreateAttribute(string prefix, string localName, string namespaceUri, string value)
		{
			if (string.IsNullOrEmpty(localName)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(localName));

			Infrastructure.Assert(NodeType == XPathNodeType.Element);

			if (_pathLength == 0)
			{
				return;
			}

			var list = Parent.DataModelValue.AsList();
			list.TryGet(Current.ParentIndex, out var entry);

			if (entry.Metadata is not null)
			{
				AddAttribute(entry.Metadata, localName, value, namespaceUri, prefix);
			}
			else
			{
				var metadata = new DataModelList();

				AddAttribute(metadata, localName, value, namespaceUri, prefix);

				list.Set(entry.Index, entry.Key, entry.Value, metadata);
			}
		}

		private static void AddAttribute(DataModelList metadata, string name, string value, string namespaceUri, string prefix)
		{
			metadata.Add(name, value, metadata: default);

			if (string.IsNullOrEmpty(namespaceUri) || string.IsNullOrEmpty(prefix))
			{
				metadata.Add(name, namespaceUri, metadata: default);
			}

			if (string.IsNullOrEmpty(prefix))
			{
				metadata.Add(name, prefix, metadata: default);
			}
		}

		public override void SetValue(string value)
		{
			if (_pathLength > 0)
			{
				Parent.DataModelValue.AsList().Set(Current.ParentIndex, Current.ParentProperty, value, metadata: default);
			}
		}

		private readonly struct Element
		{
			public readonly string?        Key;
			public readonly DataModelValue Value;

			public Element(string? key, in DataModelValue value)
			{
				Key = key;
				Value = value;
			}
		}

		internal readonly struct Node
		{
			public readonly NodeAdapter    Adapter;
			public readonly DataModelValue DataModelValue;
			public readonly DataModelList? Metadata;
			public readonly int            ParentCursor;
			public readonly int            ParentIndex;
			public readonly string?        ParentProperty;

			public Node(in DataModelValue value, NodeAdapter adapter, int parentCursor = -1, int parentIndex = -1, string? parentProperty = default, DataModelList? metadata = default)
			{
				DataModelValue = value;
				Adapter = adapter;
				ParentCursor = parentCursor;
				ParentIndex = parentIndex;
				ParentProperty = parentProperty;
				Metadata = metadata;
			}

			public bool Equals(Node node) => ParentCursor >= 0 ? ParentCursor == node.ParentCursor : DataModelValue == node.DataModelValue;

			public XPathNodeType GetNodeType()     => Adapter.GetNodeType();
			public string        GetValue()        => Adapter.GetValue(this);
			public string        GetName()         => Adapter.GetName(this);
			public string        GetPrefix()       => Adapter.GetPrefix(this);
			public string        GetNamespaceUri() => Adapter.GetNamespaceUri(this);
			public string        GetLocalName()    => Adapter.GetLocalName(this);
			public bool          IsEmptyElement()  => Adapter.IsEmptyElement(this);

			public bool GetFirstChild(out Node childNode)         => Adapter.GetFirstChild(this, out childNode);
			public bool GetNextChild(ref Node childNode)          => Adapter.GetNextChild(this, ref childNode);
			public bool GetPreviousChild(ref Node childNode)      => Adapter.GetPreviousChild(this, ref childNode);
			public bool GetFirstAttribute(out Node attributeNode) => Adapter.GetFirstAttribute(this, out attributeNode);
			public bool GetNextAttribute(ref Node attributeNode)  => Adapter.GetNextAttribute(this, ref attributeNode);
			public bool GetFirstNamespace(out Node namespaceNode) => Adapter.GetFirstNamespace(this, out namespaceNode);
			public bool GetNextNamespace(ref Node namespaceNode)  => Adapter.GetNextNamespace(this, ref namespaceNode);
		}
	}
}