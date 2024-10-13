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

using Xtate.DataModel.Runtime;

namespace Xtate;

public static class Runtime
{
	private static readonly AsyncLocal<RuntimeExecutionContext> Current = new();

	public static DataModelList DataModel => GetContext().DataModelController.DataModel;

	public static DataModelValue Arguments => GetContext().DataModelController.DataModel[@"_x"].AsListOrDefault()?["args"] ?? default;

	private static RuntimeExecutionContext GetContext()
	{
		if (Current.Value is { } context)
		{
			return context;
		}

		throw new InvalidOperationException(Resources.Exception_ContextIsNotAvailableAtThisPlace);
	}

	internal static void SetCurrentExecutionContext(RuntimeExecutionContext executionContext) => Current.Value = executionContext;

	public static bool InState(string stateId) => GetContext().InStateController.InState(Identifier.FromString(stateId));

	public static ValueTask Log(string message, DataModelValue arguments = default) => GetContext().LogController.Log(message, arguments);

	public static ValueTask SendEvent(IOutgoingEvent outgoingEvent) => GetContext().EventController.Send(outgoingEvent);

	public static ValueTask CancelEvent(SendId sendId) => GetContext().EventController.Cancel(sendId);

	public static ValueTask StartInvoke(InvokeData invokeData) => GetContext().InvokeController.Start(invokeData);

	public static ValueTask CancelInvoke(InvokeId invokeId) => GetContext().InvokeController.Cancel(invokeId);

}