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

using Xtate.IoProcessors.Http;
using Xtate.IoProcessors.NamedPipe;

namespace Xtate.Test.UnitTests.IoProcessors;

[TestClass]
public class IoProcessorOptionsCoverageTest
{
	[TestMethod]
	public void HttpOptionsExposeDefaultsAndAcceptValidValues()
	{
		var options = new HttpIoProcessorOptions
					  {
						  ListenUrl = "http://localhost:8080/",
						  PublicBaseUrl = "https://example.test/base/",
						  MaxMessageSize = 1024,
						  Timeout = TimeSpan.FromSeconds(5)
					  };

		Assert.AreEqual("http://localhost:8080/", options.ListenUrl);
		Assert.AreEqual("https://example.test/base/", options.PublicBaseUrl);
		Assert.AreEqual(1024, options.MaxMessageSize);
		Assert.AreEqual(TimeSpan.FromSeconds(5), options.Timeout);
	}

	[TestMethod]
	public void HttpOptionsRejectNullNegativeAndInvalidTimeoutValues()
	{
		var options = new HttpIoProcessorOptions();

		Assert.AreEqual("http://notexist.invalid/", options.PublicBaseUrl);
		Assert.AreEqual(0, options.MaxMessageSize);
		Assert.AreEqual(System.Threading.Timeout.InfiniteTimeSpan, options.Timeout);

		Assert.ThrowsExactly<InvalidOperationException>(() => options.ListenUrl = null);
		Assert.ThrowsExactly<InvalidOperationException>(() => options.PublicBaseUrl = null!);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => options.MaxMessageSize = -1);
		Assert.ThrowsExactly<ArgumentException>(() => options.Timeout = System.Threading.Timeout.InfiniteTimeSpan - TimeSpan.FromMilliseconds(1));
	}

	[TestMethod]
	public void NamedPipeOptionsExposeDefaultsAndAcceptValidValues()
	{
		var options = new NamedPipeIoProcessorOptions
					  {
						  Host = "localhost",
						  Name = "PipeName123",
						  MaxMessageSize = 2048,
						  Timeout = TimeSpan.FromSeconds(10)
					  };

		Assert.AreEqual("localhost", options.Host);
		Assert.AreEqual("PipeName123", options.Name);
		Assert.AreEqual(2048, options.MaxMessageSize);
		Assert.AreEqual(TimeSpan.FromSeconds(10), options.Timeout);
	}

	[TestMethod]
	public void NamedPipeOptionsRejectInvalidHostNameSizeAndTimeoutValues()
	{
		var options = new NamedPipeIoProcessorOptions();

		Assert.AreEqual(".", options.Host);
		Assert.IsNull(options.Name);
		Assert.AreEqual(0, options.MaxMessageSize);
		Assert.AreEqual(System.Threading.Timeout.InfiniteTimeSpan, options.Timeout);

		Assert.ThrowsExactly<ArgumentException>(() => options.Host = string.Empty);
		Assert.ThrowsExactly<ArgumentException>(() => options.Name = string.Empty);
		Assert.ThrowsExactly<ArgumentException>(() => options.Name = ".");
		Assert.ThrowsExactly<ArgumentException>(() => options.Name = "..");
		Assert.ThrowsExactly<ArgumentException>(() => options.Name = "contains space");
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => options.MaxMessageSize = -1);
		Assert.ThrowsExactly<ArgumentException>(() => options.Timeout = System.Threading.Timeout.InfiniteTimeSpan - TimeSpan.FromMilliseconds(1));
	}
}
