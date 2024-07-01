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

<<<<<<< Updated upstream
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
=======
>>>>>>> Stashed changes
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public sealed class DoneDataNode : IDoneData, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
{
<<<<<<< Updated upstream
	public sealed class DoneDataNode : IDoneData, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly IDoneData                           _doneData;
		private readonly IValueEvaluator?                    _contentBodyEvaluator;
		private readonly IObjectEvaluator?                   _contentExpressionEvaluator;
		private readonly ImmutableArray<DataConverter.Param> _parameterList;
		private          DocumentIdSlot                      _documentIdSlot;

		public required  Func<ValueTask<DataConverter>>      DataConverterFactory { private get; init; }

		public DoneDataNode(DocumentIdNode documentIdNode, IDoneData doneData)
		{
			Infra.Requires(doneData);

			_doneData = doneData;
			documentIdNode.SaveToSlot(out _documentIdSlot);
			_contentExpressionEvaluator = doneData.Content?.Expression?.As<IObjectEvaluator>();
			_contentBodyEvaluator = doneData.Content?.Body?.As<IValueEvaluator>();
			_parameterList = DataConverter.AsParamArray(doneData.Parameters);
		}
=======
	private readonly IValueEvaluator?                    _contentBodyEvaluator;
	private readonly IObjectEvaluator?                   _contentExpressionEvaluator;
	private readonly IDoneData                           _doneData;
	private readonly ImmutableArray<DataConverter.Param> _parameterList;
	private          DocumentIdSlot                      _documentIdSlot;

	public DoneDataNode(DocumentIdNode documentIdNode, IDoneData doneData)
	{
		_doneData = doneData;
		documentIdNode.SaveToSlot(out _documentIdSlot);
		_contentExpressionEvaluator = doneData.Content?.Expression?.As<IObjectEvaluator>();
		_contentBodyEvaluator = doneData.Content?.Body?.As<IValueEvaluator>();
		_parameterList = DataConverter.AsParamArray(doneData.Parameters);
	}

	public required Func<ValueTask<DataConverter>> DataConverterFactory { private get; [UsedImplicitly] init; }
>>>>>>> Stashed changes

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _doneData;

#endregion

<<<<<<< Updated upstream

	#region Interface IDebugEntityId

		public FormattableString EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdSlot.Value;

	#endregion

	#region Interface IDoneData
=======
#region Interface IDebugEntityId
>>>>>>> Stashed changes

	public FormattableString EntityId => @$"(#{DocumentId})";

#endregion

#region Interface IDocumentId

	public int DocumentId => _documentIdSlot.CreateValue();

#endregion

#region Interface IDoneData

<<<<<<< Updated upstream
		public async ValueTask<DataModelValue> Evaluate()
		{
			var dataConverter = await DataConverterFactory().ConfigureAwait(false);

			return await dataConverter.GetData(_contentBodyEvaluator, _contentExpressionEvaluator, nameEvaluatorList: default, _parameterList).ConfigureAwait(false);
		}
=======
	public IContent? Content => _doneData.Content;

	public ImmutableArray<IParam> Parameters => _doneData.Parameters;

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.DoneDataNode);
		bucket.AddEntity(Key.Content, Content);
		bucket.AddEntityList(Key.Parameters, Parameters);
	}

#endregion

	public async ValueTask<DataModelValue> Evaluate()
	{
		var dataConverter = await DataConverterFactory().ConfigureAwait(false);

		return await dataConverter.GetData(_contentBodyEvaluator, _contentExpressionEvaluator, nameEvaluatorList: default, _parameterList).ConfigureAwait(false);
>>>>>>> Stashed changes
	}
}