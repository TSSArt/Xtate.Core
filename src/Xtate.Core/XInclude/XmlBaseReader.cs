// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.XInclude;

public class XmlBaseReader : DelegatedXmlReader
{
    private readonly string _baseName;

    private readonly string _xmlNs;

    private Stack<(int Depth, Uri BaseUri)>? _baseUris;

    public XmlBaseReader(XmlReader xmlReader) : base(xmlReader)
    {
        var nameTable = xmlReader.NameTable;

        Infra.NotNull(nameTable);

        _baseName = nameTable.Add(@"base");
        _xmlNs = nameTable.Add(@"http://www.w3.org/XML/1998/namespace");
    }

    public required XmlResolver XmlResolver { private get; [UsedImplicitly] init; }

    public override string BaseURI => _baseUris?.Count > 0 ? _baseUris.Peek().BaseUri.ToString() : base.BaseURI;

    public override bool Read()
    {
        PreProcessNode();

        if (!base.Read())
        {
            return false;
        }

        PostProcessNode();

        return true;
    }

    public override async Task<bool> ReadAsync()
    {
        PreProcessNode();

        if (!await base.ReadAsync().ConfigureAwait(false))
        {
            return false;
        }

        PostProcessNode();

        return true;
    }

    private bool TryPeek(out int depth, [NotNullWhen(true)] out Uri? baseUri)
    {
        if (_baseUris?.Count > 0)
        {
            (depth, baseUri) = _baseUris.Peek();

            return true;
        }

        depth = 0;
        baseUri = default;

        return false;
    }

    private string? GetXmlBaseValue()
    {
        for (var ok = MoveToFirstAttribute(); ok; ok = MoveToNextAttribute())
        {
            if (ReferenceEquals(NamespaceURI, _xmlNs) && ReferenceEquals(LocalName, _baseName))
            {
                MoveToElement();

                return Value;
            }
        }

        MoveToElement();

        return default;
    }

    private void PostProcessNode()
    {
        if (NodeType == XmlNodeType.Element && GetXmlBaseValue() is { } xmlBase)
        {
            _baseUris ??= new Stack<(int Depth, Uri BaseUri)>();

            if (!TryPeek(out _, out var baseUri))
            {
                baseUri = base.BaseURI is { } uri ? new Uri(uri, UriKind.RelativeOrAbsolute) : null;
            }

            _baseUris.Push((Depth, XmlResolver.ResolveUri(baseUri!, xmlBase)));
        }
    }

    private void PreProcessNode()
    {
        if ((NodeType == XmlNodeType.EndElement || (NodeType == XmlNodeType.Element && IsEmptyElement)) && TryPeek(out var depth, out _) && depth == Depth)
        {
            Infra.NotNull(_baseUris);

            _baseUris.Pop();
        }
    }
}