#region Copyright © 2019-2021 Sergii Artemenko

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

using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Core;

namespace Xtate.DataModel.XPath
{
	internal class XPathExternalDataExpressionEvaluator : DefaultExternalDataExpressionEvaluator
	{
		public XPathExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) : base(externalDataExpression) { }

		protected override async ValueTask<DataModelValue> ParseToDataModel(IExecutionContext executionContext, Resource resource, CancellationToken token)
		{
			var content = await resource.GetContent(token).ConfigureAwait(false);

			if (content is null)
			{
				return DataModelValue.Null;
			}

			try
			{
				return XmlConverter.FromXml(content, this);
			}
			catch (XmlException ex)
			{
				await executionContext.Log(LogLevel.Warning, exception: ex, token: token).ConfigureAwait(false);

				return content.NormalizeSpaces();
			}
		}
	}
}