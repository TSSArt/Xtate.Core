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

using System;
using System.Threading.Tasks;
using Xtate.Core;

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
=======
namespace Xtate.DataModel;

public abstract class RaiseEvaluator(IRaise raise) : IRaise, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => raise;
>>>>>>> Stashed changes

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IRaise

<<<<<<< Updated upstream
	public IOutgoingEvent? OutgoingEvent => _raise.OutgoingEvent;
=======
	public virtual IOutgoingEvent? OutgoingEvent => raise.OutgoingEvent;
>>>>>>> Stashed changes

#endregion
}

<<<<<<< Updated upstream
public class DefaultRaiseEvaluator : RaiseEvaluator
{
	public DefaultRaiseEvaluator(IRaise raise) : base(raise)
	{
		Infra.NotNull(raise.OutgoingEvent);
	}

	public required Func<ValueTask<IEventController?>> EventSenderFactory { private get; init; }

	public override async ValueTask Execute()
	{
		if (await EventSenderFactory().ConfigureAwait(false) is { } eventSender)
		{
			await eventSender.Send(OutgoingEvent!).ConfigureAwait(false);
=======
public class DefaultRaiseEvaluator(IRaise raise) : RaiseEvaluator(raise)
{
	public required Func<ValueTask<IEventController?>> EventSenderFactory { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		var outgoingEvent = base.OutgoingEvent;
		Infra.NotNull(outgoingEvent);

		if (await EventSenderFactory().ConfigureAwait(false) is { } eventSender)
		{
			await eventSender.Send(outgoingEvent).ConfigureAwait(false);
>>>>>>> Stashed changes
		}
	}
}