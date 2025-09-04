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

using System.Diagnostics;
using Xtate.IoC;

namespace Xtate;

public class DebugTraceModule : Module
{
    protected override void AddServices()
    {
        AddTraceServices();
        AddDebugServices();
    }

    [Conditional("DEBUG")]
    private void AddDebugServices()
    {
        Services.AddShared<IServiceProviderActions>(SharedWithin.Container, _ => new ServiceProviderDebugger(Console.Out, leaveOpen: true));
    }

    [Conditional("TRACE")]
    private void AddTraceServices()
    {
        Services.AddImplementation<TraceLogWriter<Any>>().For<ILogWriter<Any>>();
        Services.AddShared<TraceListener>(SharedWithin.Container, _ => new ConsoleListener());
    }

    private class ConsoleListener : TextWriterTraceListener
    {
        public ConsoleListener() => Writer = Console.Out;

        protected override void Dispose(bool disposing)
        {
            Writer = null;

            base.Dispose(disposing);
        }
    }
}