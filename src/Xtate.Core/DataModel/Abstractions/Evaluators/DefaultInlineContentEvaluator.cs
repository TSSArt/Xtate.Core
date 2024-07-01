<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2024 Sergii Artemenko
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

public abstract class InlineContentEvaluator : IInlineContent, IObjectEvaluator, IStringEvaluator, IAncestorProvider
{
	private readonly IInlineContent _inlineContent;

	protected InlineContentEvaluator(IInlineContent inlineContent)
	{
		Infra.Requires(inlineContent);

		_inlineContent = inlineContent;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _inlineContent;
=======
namespace Xtate.DataModel;

public abstract class InlineContentEvaluator(IInlineContent inlineContent) : IInlineContent, IObjectEvaluator, IStringEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => inlineContent;
>>>>>>> Stashed changes

#endregion

#region Interface IInlineContent

<<<<<<< Updated upstream
	public virtual string? Value => _inlineContent.Value;
=======
	public virtual string? Value => inlineContent.Value;
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
public class DefaultInlineContentEvaluator : InlineContentEvaluator
{
	private DataModelValue _parsedValue;

	public DefaultInlineContentEvaluator(IInlineContent inlineContent) : base(inlineContent) { }

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
public class DefaultInlineContentEvaluator(IInlineContent inlineContent) : InlineContentEvaluator(inlineContent)
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