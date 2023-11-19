#region Copyright © 2019-2022 Sergii Artemenko

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

namespace Xtate.Core.IoC;

public class ForwardingImplementationEntry : ImplementationEntry
{
	private readonly ServiceProvider _serviceProvider;

	public ForwardingImplementationEntry(ServiceProvider serviceProvider, object factory) : base(factory) => _serviceProvider = serviceProvider;

	private ForwardingImplementationEntry(ServiceProvider serviceProvider, ForwardingImplementationEntry sourceEntry) : base(sourceEntry) => _serviceProvider = serviceProvider;

	protected override IServiceProvider ServiceProvider => _serviceProvider;

	internal override ImplementationEntry CreateNew2(ServiceProvider serviceProvider) => new ForwardingImplementationEntry(serviceProvider, this);

	internal override ImplementationEntry CreateNew2(ServiceProvider serviceProvider, object factory) => new ForwardingImplementationEntry(serviceProvider, factory);
}