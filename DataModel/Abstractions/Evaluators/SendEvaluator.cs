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

public abstract class SendEvaluator(ISend send) : IExecEvaluator, ISend, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => send;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ISend

	public virtual IContent? Content => send.Content;

	public virtual IValueExpression? DelayExpression => send.DelayExpression;

	public virtual int? DelayMs => send.DelayMs;

	public virtual string? EventName => send.EventName;

	public virtual IValueExpression? EventExpression => send.EventExpression;

	public virtual string? Id => send.Id;

	public virtual ILocationExpression? IdLocation => send.IdLocation;

	public virtual ImmutableArray<ILocationExpression> NameList => send.NameList;

	public virtual ImmutableArray<IParam> Parameters => send.Parameters;

	public virtual Uri? Target => send.Target;

	public virtual IValueExpression? TargetExpression => send.TargetExpression;

	public virtual Uri? Type => send.Type;

	public virtual IValueExpression? TypeExpression => send.TypeExpression;

#endregion
}