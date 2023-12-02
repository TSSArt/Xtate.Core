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

public abstract class RaiseEvaluator : IRaise, IExecEvaluator, IAncestorProvider
{
	private readonly IRaise _raise;

	protected RaiseEvaluator(IRaise raise)
	{
		Infra.Requires(raise);

		_raise = raise;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _raise;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IRaise

	public IOutgoingEvent? OutgoingEvent => _raise.OutgoingEvent;

#endregion
}

public class DefaultRaiseEvaluator : RaiseEvaluator
{
	public DefaultRaiseEvaluator(IRaise raise) : base(raise)
	{
		Infra.NotNull(raise.OutgoingEvent);
	}

	public required Func<ValueTask<IEventController?>> EventSenderFactory { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		if (await EventSenderFactory().ConfigureAwait(false) is { } eventSender)
		{
			await eventSender.Send(OutgoingEvent!).ConfigureAwait(false);
		}
	}
}