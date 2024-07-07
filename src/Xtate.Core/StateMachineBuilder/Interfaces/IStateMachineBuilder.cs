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

public interface IStateMachineBuilder
{
	IStateMachine Build();

	void SetInitial(ImmutableArray<IIdentifier> initial);
	void AddState(IState state);
	void AddParallel(IParallel parallel);
	void AddFinal(IFinal final);
	void SetDataModel(IDataModel dataModel);
	void SetScript(IScript script);
	void SetName(string name);
	void SetDataModelType(string dataModelType);
	void SetBindingType(BindingType bindingType);
	void SetPersistenceLevel(PersistenceLevel persistenceLevel);
	void SetSynchronousEventProcessing(bool value);
	void SetExternalQueueSize(int size);
	void SetUnhandledErrorBehaviour(UnhandledErrorBehaviour unhandledErrorBehaviour);
}