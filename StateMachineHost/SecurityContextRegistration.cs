// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.Core;

public sealed class SecurityContextRegistration : IAsyncDisposable
{
    private readonly AsyncLocal<SecurityContext> _asyncLocal;

    private readonly SecurityContext _newContext;

    private readonly SecurityContext? _parentContext;

    internal SecurityContextRegistration(AsyncLocal<SecurityContext> asyncLocal, SecurityContextType securityContextType)
    {
        _asyncLocal = asyncLocal;
        _parentContext = asyncLocal.Value;
        asyncLocal.Value = _newContext = (_parentContext ?? SecurityContext.FullAccess).CreateNested(securityContextType);
    }

#region Interface IAsyncDisposable

    public ValueTask DisposeAsync()
    {
        Infra.Assert(_asyncLocal.Value == _newContext);

        _asyncLocal.Value = _parentContext!;

        return default;
    }

#endregion
}