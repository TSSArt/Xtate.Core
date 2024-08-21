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

namespace Xtate.Builder;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class CancelBuilder : BuilderBase, ICancelBuilder
{
	private string?           _sendId;
	private IValueExpression? _sendIdExpression;

#region Interface ICancelBuilder

	public ICancel Build() => new CancelEntity { Ancestor = Ancestor, SendId = _sendId, SendIdExpression = _sendIdExpression };

	public void SetSendId(string sendId)
	{
		Infra.RequiresNonEmptyString(sendId);

		_sendId = sendId;
	}

	public void SetSendIdExpression(IValueExpression sendIdExpression)
	{
		Infra.Requires(sendIdExpression);

		_sendIdExpression = sendIdExpression;
	}

#endregion
}