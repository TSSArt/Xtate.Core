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

namespace Xtate.DataModel.Runtime;

public abstract class RuntimeAction : IExecutableEntity
{
    public static RuntimeAction GetAction(Action action)
    {
        Infra.Requires(action);

        return new ActionSync(action);
    }

    public static RuntimeAction GetAction(Func<ValueTask> action)
    {
        Infra.Requires(action);

        return new ActionAsync(action);
    }

    public abstract ValueTask DoAction();

    private sealed class ActionSync(Action action) : RuntimeAction
    {
        public override ValueTask DoAction()
        {
            action();

            return default;
        }
    }

    private sealed class ActionAsync(Func<ValueTask> action) : RuntimeAction
    {
        public override ValueTask DoAction() => action();
    }
}