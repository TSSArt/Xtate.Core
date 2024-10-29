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

using System.Xml.XPath;

namespace Xtate.DataModel.XPath;

public class InFunction() : XPathFunctionBase(XPathResultType.Boolean, XPathResultType.Any)
{
	public class Provider() : XPathFunctionProviderBase<InFunction>(string.Empty, name: @"In");

	private IInStateController _inStateController;

	public required Func<ValueTask<IInStateController>> InStateControllerFactory { private get; [UsedImplicitly] init; }

	public override async ValueTask Initialize()
	{
		_inStateController = await InStateControllerFactory().ConfigureAwait(false);

		await base.Initialize().ConfigureAwait(false);
	}

	protected override object Invoke(object[] args)
	{
		if (args is [string stateId])
		{
			return InState(stateId);
		}

		if (args is [XPathNodeIterator iterator])
		{
			if (!iterator.MoveNext())
			{
				return false;
			}

			do
			{
				if (!InState(iterator.Current?.Value))
				{
					return false;
				}
			}
			while (iterator.MoveNext());

			return true;
		}

		return false;
	}

	private bool InState(string stateId) => !string.IsNullOrEmpty(stateId) && _inStateController.InState((Identifier) stateId);
}