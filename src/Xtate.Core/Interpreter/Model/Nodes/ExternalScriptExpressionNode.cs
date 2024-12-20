﻿// Copyright © 2019-2024 Sergii Artemenko
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

public sealed class ExternalScriptExpressionNode : IExternalScriptExpression, IExternalScriptConsumer, IStoreSupport, IAncestorProvider
{
	private readonly IExternalScriptExpression _externalScriptExpression;

	private string? _content;

	public ExternalScriptExpressionNode(IExternalScriptExpression externalScriptExpression)
	{
		Infra.NotNull(externalScriptExpression.Uri);

		_externalScriptExpression = externalScriptExpression;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _externalScriptExpression;

#endregion

#region Interface IExternalScriptConsumer

	public void SetContent(string content)
	{
		_content = content;

		if (_externalScriptExpression.Is<IExternalScriptConsumer>(out var externalScript))
		{
			externalScript.SetContent(content);
		}
	}

#endregion

#region Interface IExternalScriptExpression

	public Uri Uri => _externalScriptExpression.Uri!;

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.ExternalScriptExpressionNode);
		bucket.Add(Key.Uri, Uri);
		bucket.Add(Key.Content, _content);
	}

#endregion
}