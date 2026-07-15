// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.ExternalServices.DependencyInjection;
using Xtate.IoC;
using Xtate.IoProcessors.Http.DependencyInjection;
using Xtate.IoProcessors.NamedPipe.DependencyInjection;

namespace Xtate.Test.UnitTests.Common;

[TestClass]
public class DependencyInjectionModuleCoverageTest
{
	[TestMethod]
	public void TransportAndExternalServiceModulesRegisterWithFreshCollections()
	{
		var externalServices = new ServiceCollection();
		externalServices.AddModule<ExternalServicesModule>();
		_ = externalServices.BuildProvider();

		var http = new ServiceCollection();
		http.AddModule<HttpIoProcessorModule>();
		_ = http.BuildProvider();

		var namedPipe = new ServiceCollection();
		namedPipe.AddModule<NamedPipeIoProcessorModule>();
		_ = namedPipe.BuildProvider();
	}
}
