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

using System.Xml;
using Xtate.XInclude;

namespace Xtate.Core;

public class ScxmlXmlResolver : XmlResolver, IXIncludeXmlResolver
{
#region Interface IXIncludeXmlResolver

	object IXIncludeXmlResolver.GetEntity(Uri uri,
										  string? accept,
										  string? acceptLanguage,
										  Type? ofObjectToReturn) =>
		GetEntityAsync(uri, accept, acceptLanguage, ofObjectToReturn).SynchronousGetResult();

	Task<object?> IXIncludeXmlResolver.GetEntityAsync(Uri uri,
													  string? accept,
													  string? acceptLanguage,
													  Type? ofObjectToReturn) =>
		GetEntityAsync(uri, accept, acceptLanguage, ofObjectToReturn).AsTask()!;

#endregion

	public sealed override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn) =>
		GetEntityAsync(absoluteUri, accept: default, acceptLanguage: default, ofObjectToReturn).SynchronousGetResult();

	public sealed override Task<object> GetEntityAsync(Uri absoluteUri, string? role, Type? ofObjectToReturn) =>
		GetEntityAsync(absoluteUri, accept: default, acceptLanguage: default, ofObjectToReturn).AsTask();

	protected virtual ValueTask<object> GetEntityAsync(Uri uri,
													   string? accept,
													   string? acceptLanguage,
													   Type? ofObjectToReturn) =>
		throw new NotSupportedException(Resources.Exception_LoadingExternalResourcesDoesNotSupported);
}