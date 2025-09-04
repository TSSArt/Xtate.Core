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

using Xtate.IoC;

namespace Xtate.Core;

public class AncestorTracker : IServiceProviderActions, IServiceProviderDataActions
{
    private readonly AsyncLocal<Container?> _local = new();

    private Container CurrentContainer => _local.Value ??= [];

#region Interface IServiceProviderActions

    public IServiceProviderDataActions? RegisterServices() => null;

    public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey) => null;

    public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey) => null;

    public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey) => typeKey.IsEmptyArg ? this : null;

    public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey) => typeKey.IsEmptyArg ? this : null;

#endregion

#region Interface IServiceProviderDataActions

    [ExcludeFromCodeCoverage]
    public void RegisterService(ServiceEntry serviceEntry) { }

    [ExcludeFromCodeCoverage]
    public void ServiceRequesting<T, TArg>(TArg argument) { }

    [ExcludeFromCodeCoverage]
    public void ServiceRequested<T, TArg>(T? instance) { }

    public void FactoryCalling<T, TArg>(TArg argument) => CurrentContainer.Add((typeof(T), null));

    public void FactoryCalled<T, TArg>(T? instance)
    {
        var container = CurrentContainer;

        for (var i = 0; i < container.Count; i ++)
        {
            var (type, ancestor) = container[i];

            if (type == typeof(T))
            {
                container[i] = default;

                if (ancestor is AncestorFactory<T> ancestorFactory)
                {
                    ancestorFactory.SetValue(instance);
                }
            }
        }

        container.RemoveAll(static p => p.Type is null);
    }

#endregion

    public bool TryCaptureAncestor(Type ancestorType, object ancestorFactory)
    {
        var container = CurrentContainer;

        for (var i = 0; i < container.Count; i ++)
        {
            var (type, ancestor) = container[i];

            if (type == ancestorType)
            {
                if (ancestor is null)
                {
                    container[i] = (type, ancestorFactory);
                }
                else
                {
                    container.Add((type, ancestorFactory));
                }

                return true;
            }
        }

        return false;
    }

    private class Container : List<(Type Type, object? Ancestor)>;
}