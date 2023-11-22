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

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

[Obsolete]
public abstract class DoneDataEvaluator : IObjectEvaluator, IDoneData, IAncestorProvider
{
	private readonly IDoneData _doneData;

	protected DoneDataEvaluator(IDoneData doneData)
	{
		Infra.Requires(doneData);

		_doneData = doneData;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _doneData;

#endregion

#region Interface IDoneData

	public virtual IContent?              Content    => _doneData.Content;
	public virtual ImmutableArray<IParam> Parameters => _doneData.Parameters;

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion
}

[Obsolete]
public class DefaultDoneDataEvaluator : DoneDataEvaluator
{
	private readonly IValueEvaluator?                    _contentBodyEvaluator;
	private readonly IObjectEvaluator?                   _contentExpressionEvaluator;
	private readonly ImmutableArray<DataConverter.Param> _parameterList;

	public DefaultDoneDataEvaluator(IDoneData doneData) : base(doneData)
	{
		_contentExpressionEvaluator = doneData.Content?.Expression?.As<IObjectEvaluator>();
		_contentBodyEvaluator = doneData.Content?.Body?.As<IValueEvaluator>();
		_parameterList = DataConverter.AsParamArray(doneData.Parameters);
	}

	public required Func<ValueTask<DataConverter>> DataConverterFactory { private get; init; }

	public override async ValueTask<IObject> EvaluateObject()
	{
		var dataConverter = await DataConverterFactory().ConfigureAwait(false);

		return await dataConverter.GetData(_contentBodyEvaluator, _contentExpressionEvaluator, nameEvaluatorList: default, _parameterList).ConfigureAwait(false);
	}
}