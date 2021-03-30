#region Copyright © 2019-2021 Sergii Artemenko

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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	public enum SecurityContextType
	{
		NoAccess,
		NewStateMachine,
		NewTrustedStateMachine,
		InvokedService
	}

	public interface ISecurityContext
	{
		TaskFactory IoBoundTaskFactory { get; }

		ISecurityContext CreateNested(SecurityContextType type, DeferredFinalizer finalizer);

		ValueTask SetValue<T>(object key,
							  object subKey,
							  [DisallowNull] T value,
							  ValueOptions options);

		bool TryGetValue<T>(object key, object subKey, [NotNullWhen(true)] out T? value);
	}
}