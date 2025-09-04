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

using System.Xml;
using Xtate.Scxml;

namespace Xtate.Core;

public class ErrorProcessorService<TSource> : IErrorProcessorService<TSource>
{
    public required IErrorProcessor ErrorProcessor { private get; [UsedImplicitly] init; }

    public required ILineInfoRequired? LineInfoRequired { private get; [UsedImplicitly] init; }

#region Interface IErrorProcessorService<TSource>

    public virtual void AddError(object? entity, string message, Exception? exception = default)
    {
        Infra.Requires(message);

        if (LineInfoRequired?.LineInfoRequired ?? false)
        {
            if (entity.Is<IXmlLineInfo>(out var xmlLineInfo) && xmlLineInfo.HasLineInfo())
            {
                ErrorProcessor.AddError(new ErrorItem(typeof(TSource), message, exception, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition));

                return;
            }

            if (exception is XmlException { LineNumber: > 0 } xmlException)
            {
                ErrorProcessor.AddError(new ErrorItem(typeof(TSource), message, exception, xmlException.LineNumber, xmlException.LinePosition));

                return;
            }
        }

        ErrorProcessor.AddError(new ErrorItem(typeof(TSource), message, exception));
    }

#endregion
}