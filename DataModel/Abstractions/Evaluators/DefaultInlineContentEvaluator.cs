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

public abstract class InlineContentEvaluator(IInlineContent inlineContent) : IInlineContent, IObjectEvaluator, IStringEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => inlineContent;

#endregion

#region Interface IInlineContent

	public virtual string? Value => inlineContent.Value;

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion

#region Interface IStringEvaluator

	public virtual ValueTask<string> EvaluateString() => new(Value ?? string.Empty);

#endregion
}

public class DefaultInlineContentEvaluator(IInlineContent inlineContent) : InlineContentEvaluator(inlineContent)
{
	private DataModelValue _contentValue;

	private Exception? _parseException;

	public required Func<ValueTask<ILogger<IInlineContent>>> LoggerFactory { private get; [UsedImplicitly] init; }

	public override async ValueTask<IObject> EvaluateObject()
	{
		if (_contentValue.IsUndefined() || _parseException is not null)
		{
			try
			{
				_contentValue = ParseToDataModel();
			}
			catch (Exception exception)
			{
				_parseException = exception;

				var logger = await LoggerFactory().ConfigureAwait(false);
				await logger.Write(Level.Warning, eventId: 1, message: Resources.Exception_FailedToParseInlineContent, exception).ConfigureAwait(false);
			}

			_contentValue.MakeDeepConstant();
		}

		return _contentValue;
	}

	protected virtual DataModelValue ParseToDataModel() => DataModelValue.FromString(base.Value);
}