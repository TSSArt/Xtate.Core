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

public class NoDataModelHandler : IDataModelHandler
{
	public bool CaseInsensitive => false;

	public ImmutableDictionary<string, string> DataModelVars => ImmutableDictionary<string, string>.Empty;

	public void Process(ref IExecutableEntity executableEntity) => throw new InvalidOperationException();

	public void Process(ref IValueExpression valueExpression) => throw new InvalidOperationException();

	public void Process(ref ILocationExpression locationExpression) => throw new InvalidOperationException();

	public void Process(ref IConditionExpression conditionExpression) => throw new InvalidOperationException();

	public void Process(ref IContentBody contentBody) => throw new InvalidOperationException();

	public void Process(ref IInlineContent inlineContent) => throw new InvalidOperationException();

	public void Process(ref IExternalDataExpression externalDataExpression) => throw new InvalidOperationException();

	public string ConvertToText(DataModelValue value) => value.ToString(provider: null);
}