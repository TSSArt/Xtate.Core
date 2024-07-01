<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2023 Sergii Artemenko
// 
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
#endregion

using System.Threading.Tasks;
using Xtate.Core;

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
=======
namespace Xtate.DataModel;

public abstract class ContentBodyEvaluator(IContentBody contentBody) : IContentBody, IObjectEvaluator, IStringEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => contentBody;
>>>>>>> Stashed changes

#endregion

#region Interface IContentBody

<<<<<<< Updated upstream
	public virtual string? Value => _contentBody.Value;
=======
	public virtual string? Value => contentBody.Value;
>>>>>>> Stashed changes

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion

#region Interface IStringEvaluator

	public virtual ValueTask<string> EvaluateString() => new(Value ?? string.Empty);

#endregion
}

<<<<<<< Updated upstream
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

		return new(_parsedValue.CloneAsWritable());
	}

	protected virtual DataModelValue ParseToDataModel() => Value;
=======
public class DefaultContentBodyEvaluator(IContentBody contentBody) : ContentBodyEvaluator(contentBody)
{
	private DataModelValue _contentValue;

	public override ValueTask<IObject> EvaluateObject()
	{
		if (_contentValue.IsUndefined())
		{
			_contentValue = ParseToDataModel();
			_contentValue.MakeDeepConstant();
		}

		return new ValueTask<IObject>(_contentValue);
	}

	protected virtual DataModelValue ParseToDataModel() => DataModelValue.FromString(base.Value);
>>>>>>> Stashed changes
}