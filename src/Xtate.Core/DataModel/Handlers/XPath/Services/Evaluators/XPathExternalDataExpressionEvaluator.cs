// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.DataModel.Services;
using Xtate.DataModel.XPath.Internal;
using Xtate.DataTypes;
using Xtate.ResourceLoaders;
using Xtate.StateMachine;

namespace Xtate.DataModel.XPath.Services;

[InstantiatedByIoC]
public class XPathExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) : DefaultExternalDataExpressionEvaluator(externalDataExpression)
{
	private const string MediaTypeApplicationXml = @"application/xml";

	private const string MediaTypeTextXml = @"text/xml";

	public required XPathXmlParserContextFactory XPathXmlParserContextFactory { private get; [SetByIoC] init; }

	protected override async ValueTask<DataModelValue> ParseToDataModel(Resource resource)
	{
		var mediaType = resource.ContentType?.MediaType;

		if (mediaType is MediaTypeApplicationXml or MediaTypeTextXml)
		{
			var stream = await resource.GetStream(true).ConfigureAwait(false);
			var context = XPathXmlParserContextFactory.CreateContext(this);

			return await XmlConverter.FromXmlStreamAsync(stream, context).ConfigureAwait(false);
		}

		throw new XPathDataModelException(string.Format(Resources.Exception_Unrecognized_MediaType, mediaType));
	}
}