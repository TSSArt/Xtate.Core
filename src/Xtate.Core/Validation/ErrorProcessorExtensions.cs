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

using Xtate.IoC;

namespace Xtate.Core;

public static class ErrorProcessorExtensions
{
	public static void RegisterErrorProcessor(this IServiceCollection services)
	{
		if (services.IsRegistered<IErrorProcessorService<Any>>())
		{
			return;
		}

		services.AddSharedImplementationSync<DefaultErrorProcessor>(SharedWithin.Container).For<IErrorProcessor>();
		services.AddImplementationSync<ErrorProcessorService<Any>>().For<IErrorProcessorService<Any>>();
		services.AddImplementation<StateMachineValidator>().For<IStateMachineValidator>();
	}

	public static void AddError11<T>(this IErrorProcessor? errorProcessor,
									 object? entity,
									 string message,
									 Exception? exception = default) =>
		AddError11(errorProcessor, typeof(T), entity, message, exception);

	public static void AddError11(this IErrorProcessor? errorProcessor,
								  Type source,
								  object? entity,
								  string message,
								  Exception? exception = default)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (message is null) throw new ArgumentNullException(nameof(message));

		//TODO:
		/*
		errorProcessor ??= DefaultErrorProcessor.Instance;

		if (errorProcessor.LineInfoRequired)
		{
			if (entity.Is<IXmlLineInfo>(out var xmlLineInfo) && xmlLineInfo.HasLineInfo())
			{
				errorProcessor.AddError(new ErrorItem(source, message, exception, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition));

				return;
			}

			if (exception is XmlException { LineNumber: > 0 } xmlException)
			{
				errorProcessor.AddError(new ErrorItem(source, message, exception, xmlException.LineNumber, xmlException.LinePosition));

				return;
			}
		}

		errorProcessor.AddError(new ErrorItem(source, message, exception));*/
	}
}