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

namespace Xtate.DataModel;

public interface IDataModelHandler
{
	bool CaseInsensitive { get; }

	ImmutableDictionary<string, string> DataModelVars { get; }

	void Process(ref IExecutableEntity executableEntity);

	void Process(ref IValueExpression valueExpression);

	void Process(ref ILocationExpression locationExpression);

	void Process(ref IConditionExpression conditionExpression);

	void Process(ref IContentBody contentBody);

	void Process(ref IInlineContent inlineContent);

	void Process(ref IExternalDataExpression externalDataExpression);

	string ConvertToText(DataModelValue value);
}