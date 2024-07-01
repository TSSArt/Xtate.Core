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

<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/DataModel/Abstractions/Interfaces/IDataModelHandlerFactoryActivator.cs
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public interface IDataModelHandlerFactoryActivator
	{
		ValueTask<IDataModelHandler> CreateHandler(ServiceLocator serviceLocator,
												   string dataModelType,
												   IErrorProcessor? errorProcessor,
												   CancellationToken token);
	}
}
=======
global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Collections.Concurrent;
global using System.Diagnostics.CodeAnalysis;
global using System.Threading.Tasks;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using JetBrains.Annotations;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using Moq;
>>>>>>> Stashed changes:src/Xtate.Core/test/Xtate.Core.Test/GlobalUsing.cs
