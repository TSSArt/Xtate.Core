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

namespace Xtate.DataModel;

public abstract class ContentBodyEvaluator : IContentBody, IObjectEvaluator, IStringEvaluator, IAncestorProvider
{
	private readonly IContentBody _contentBody;

	protected ContentBodyEvaluator(IContentBody contentBody)
	{
		Infra.Requires(contentBody);

		_contentBody = contentBody;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _contentBody;

#endregion

#region Interface IContentBody

	public virtual string? Value => _contentBody.Value;

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion

#region Interface IStringEvaluator

	public virtual ValueTask<string> EvaluateString() => new(Value ?? string.Empty);

#endregion
}

public class DefaultContentBodyEvaluator : ContentBodyEvaluator
{
	private DataModelValue _parsedValue;

	public DefaultContentBodyEvaluator(IContentBody contentBody) : base(contentBody) { }

	public override ValueTask<IObject> EvaluateObject()
	{
		if (_parsedValue.IsUndefined())
		{
			_parsedValue = ParseToDataModel();
			_parsedValue.MakeDeepConstant();
		}

		return new ValueTask<IObject>(_parsedValue.CloneAsWritable());
	}

	protected virtual DataModelValue ParseToDataModel() => Value;
}