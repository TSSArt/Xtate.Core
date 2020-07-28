#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultDoneDataEvaluator : IObjectEvaluator, IDoneData, IAncestorProvider
	{
		private readonly IValueEvaluator?             _contentBodyEvaluator;
		private readonly IObjectEvaluator?            _contentExpressionEvaluator;
		private readonly DoneDataEntity               _doneData;
		private readonly ImmutableArray<DefaultParam> _parameterList;

		public DefaultDoneDataEvaluator(in DoneDataEntity doneData)
		{
			_doneData = doneData;

			_contentExpressionEvaluator = doneData.Content?.Expression?.As<IObjectEvaluator>();
			_contentBodyEvaluator = doneData.Content?.Body?.As<IValueEvaluator>();
			_parameterList = doneData.Parameters.AsArrayOf<IParam, DefaultParam>();
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _doneData.Ancestor;

	#endregion

	#region Interface IDoneData

		public IContent? Content => _doneData.Content;

		public ImmutableArray<IParam> Parameters => _doneData.Parameters;

	#endregion

	#region Interface IObjectEvaluator

		public async ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			return await DataConverter.GetData(_contentBodyEvaluator, _contentExpressionEvaluator, nameEvaluatorList: default, _parameterList, executionContext, token).ConfigureAwait(false);
		}

	#endregion
	}
}