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

public class DefaultContentBodyEvaluator(IContentBody contentBody) : ContentBodyEvaluator(contentBody)
{
	private DataModelValue _contentValue;

	private Exception? _parseException;

	public required Deferred<ILogger<IContentBody>> Logger { private get; [UsedImplicitly] init; }

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

				var logger = await Logger().ConfigureAwait(false);
				await logger.Write(Level.Warning, eventId: 1, Resources.Exception_FailedToParseContentBody, exception).ConfigureAwait(false);
			}

			_contentValue.MakeDeepConstant();
		}

		return _contentValue;
	}

	protected virtual DataModelValue ParseToDataModel() => DataModelValue.FromString(base.Value);
}