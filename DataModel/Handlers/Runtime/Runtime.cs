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

	public static DataModelList DataModel => GetContext().DataModelController?.DataModel ?? DataModelList.Empty;

	private static RuntimeExecutionContext GetContext()
	{
		if (Current.Value is { } context)
		{
			return context;
		}

		throw new InfrastructureException(Resources.Exception_ContextIsNotAvailableAtThisPlace);
	}

	internal static void SetCurrentExecutionContext(RuntimeExecutionContext executionContext) => Current.Value = executionContext;

	public static bool InState(string stateId) => GetContext().InStateController?.InState(Identifier.FromString(stateId)) ?? false;

	public static ValueTask Log(string message, DataModelValue arguments = default) => GetContext().LogController?.Log(message, arguments) ?? default;

	public static ValueTask SendEvent(IOutgoingEvent outgoingEvent) => GetContext().EventController?.Send(outgoingEvent) ?? default;

	public static ValueTask CancelEvent(SendId sendId) => GetContext().EventController?.Cancel(sendId) ?? default;

	public static ValueTask StartInvoke(InvokeData invokeData) => GetContext().InvokeController?.Start(invokeData) ?? default;

	public static ValueTask CancelInvoke(InvokeId invokeId) => GetContext().InvokeController?.Cancel(invokeId) ?? default;
}