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

using Xtate.ExternalServices.HttpClient.Internal;

namespace Xtate.Test;

[TestClass]
public class HttpClientServiceOptionsCoverageTest
{
	[TestMethod]
	public void CreateDefaultReturnsNewOptionsWithBuiltInMimeTypeHandlers()
	{
		var options = HttpClientServiceOptions.CreateDefault();
		var secondOptions = HttpClientServiceOptions.CreateDefault();

		CollectionAssert.AreEqual(
			new[]
			{
				HttpClientFormUrlEncodedHandler.Instance,
				HttpClientJsonHandler.Instance,
				HttpClientXmlHandler.Instance
			},
			options.MimeTypeHandlers);

		Assert.AreNotSame(options, secondOptions);
		Assert.AreNotSame(options.MimeTypeHandlers, secondOptions.MimeTypeHandlers);
	}

	[TestMethod]
	public void MimeTypeHandlersListCanBeCustomizedPerOptionsInstance()
	{
		var options = HttpClientServiceOptions.CreateDefault();
		var secondOptions = HttpClientServiceOptions.CreateDefault();
		var customHandler = new CustomMimeTypeHandler();

		options.MimeTypeHandlers.Clear();
		options.MimeTypeHandlers.Add(customHandler);

		CollectionAssert.AreEqual(new HttpClientMimeTypeHandler[] { customHandler }, options.MimeTypeHandlers);
		Assert.AreEqual(3, secondOptions.MimeTypeHandlers.Count);
	}

	private sealed class CustomMimeTypeHandler : HttpClientMimeTypeHandler;
}
