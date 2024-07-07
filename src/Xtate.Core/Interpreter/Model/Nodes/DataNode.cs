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
using Xtate.Persistence;

namespace Xtate.Core;

public class DataNode : IData, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
{
	private readonly IData _data;

	private DocumentIdSlot _documentIdSlot;

	public DataNode(DocumentIdNode documentIdNode, IData data)
	{
		_data = data;

		documentIdNode.SaveToSlot(out _documentIdSlot);

		SourceEvaluator = data.Source?.As<IObjectEvaluator>();
		ExpressionEvaluator = data.Expression?.As<IObjectEvaluator>();
		InlineContentEvaluator = data.InlineContent?.As<IObjectEvaluator>();
	}

	public IObjectEvaluator? SourceEvaluator        { get; }
	public IObjectEvaluator? ExpressionEvaluator    { get; }
	public IObjectEvaluator? InlineContentEvaluator { get; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _data;

#endregion

#region Interface IData

	public string?                  Id            => _data.Id;
	public IValueExpression?        Expression    => _data.Expression;
	public IExternalDataExpression? Source        => _data.Source;
	public IInlineContent?          InlineContent => _data.InlineContent;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

#endregion

#region Interface IDocumentId

	public int DocumentId => _documentIdSlot.CreateValue();

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.DataNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.Add(Key.Id, Id);
		bucket.AddEntity(Key.Source, Source);
		bucket.AddEntity(Key.Expression, Expression);
		bucket.Add(Key.InlineContent, InlineContent?.Value);
	}

#endregion
}