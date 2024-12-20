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

using Xtate.IoC;

namespace Xtate.DataModel;

public class DataModelHandlerBaseModule : Module<CustomActionModule, LoggingModule, ToolsModule>
{
	protected override void AddServices()
	{
		Services.AddImplementation<Stub>().For<IExternalCommunication>(Option.IfNotRegistered);

		Services.AddTypeSync<DefaultAssignEvaluator, IAssign>();
		Services.AddTypeSync<DefaultCancelEvaluator, ICancel>();
		Services.AddTypeSync<DefaultContentBodyEvaluator, IContentBody>();
		Services.AddTypeSync<DefaultCustomActionEvaluator, ICustomAction>();
		Services.AddTypeSync<DefaultExternalDataExpressionEvaluator, IExternalDataExpression>();
		Services.AddTypeSync<DefaultForEachEvaluator, IForEach>();
		Services.AddTypeSync<DefaultIfEvaluator, IIf>();
		Services.AddTypeSync<DefaultInlineContentEvaluator, IInlineContent>();
		Services.AddTypeSync<DefaultLogEvaluator, ILog>();
		Services.AddTypeSync<DefaultRaiseEvaluator, IRaise>();
		Services.AddTypeSync<DefaultScriptEvaluator, IScript>();
		Services.AddTypeSync<DefaultSendEvaluator, ISend>();

		Services.AddImplementation<CaseSensitivity>().For<ICaseSensitivity>();
		Services.AddImplementation<LogController>().For<ILogController>();
		Services.AddImplementation<EventController>().For<IEventController>();

		Services.AddType<DataConverter>(Option.IfNotRegistered);
	}

	private class Stub : IExternalCommunication
	{
	#region Interface IExternalCommunication

		ValueTask<SendStatus> IExternalCommunication.TrySend(IOutgoingEvent outgoingEvent) => throw Infra.Fail<Exception>();

		ValueTask IExternalCommunication.Cancel(SendId sendId) => throw Infra.Fail<Exception>();

	#endregion
	}
}