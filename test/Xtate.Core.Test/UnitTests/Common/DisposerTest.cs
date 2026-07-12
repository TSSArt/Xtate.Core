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

namespace Xtate.Test;

[TestClass]
public class DisposerTest
{
	[TestMethod]
	public void IsDisposable_WithIDisposable_ShouldReturnTrue()
	{
		// Arrange
		IDisposable disposable = new MockDisposable();

		// Act
		var result = Disposer.IsDisposable(disposable);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsDisposable_WithIAsyncDisposable_ShouldReturnTrue()
	{
		// Arrange
		IAsyncDisposable asyncDisposable = new MockAsyncDisposable();

		// Act
		var result = Disposer.IsDisposable(asyncDisposable);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsDisposable_WithNonDisposable_ShouldReturnFalse()
	{
		// Arrange
		var notDisposable = new NotDisposable();

		// Act
		var result = Disposer.IsDisposable(notDisposable);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsDisposable_WithNull_ShouldReturnFalse()
	{
		// Act
		var result = Disposer.IsDisposable<object?>(null);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Dispose_WithIDisposable_ShouldCallDispose()
	{
		// Arrange
		var mockDisposable = new MockDisposable();

		// Act
		Disposer.Dispose(mockDisposable);

		// Assert
		Assert.IsTrue(mockDisposable.IsDisposed);
	}

	[TestMethod]
	public void Dispose_WithIAsyncDisposable_ShouldCallDisposeAsync()
	{
		// Arrange
		var mockAsyncDisposable = new MockAsyncDisposable();

		// Act
		Disposer.Dispose(mockAsyncDisposable);

		// Assert
		Assert.IsTrue(mockAsyncDisposable.IsAsyncDisposed);
	}

	[TestMethod]
	public void Dispose_WithBothDisposable_ShouldPreferSync()
	{
		// Arrange
		var bothDisposable = new BothDisposable();

		// Act
		Disposer.Dispose(bothDisposable);

		// Assert
		Assert.IsTrue(bothDisposable.IsSyncDisposed);
	}

	[TestMethod]
	[ExcludeFromCodeCoverage]
	public void Dispose_WithNonDisposable_ShouldNotThrow()
	{
		// Arrange
		var notDisposable = new NotDisposable();

		// Act & Assert
		try
		{
			Disposer.Dispose(notDisposable);
			Assert.IsTrue(true);
		}
		catch (Exception ex)
		{
			Assert.Fail($"Unexpected exception: {ex.Message}");
		}
	}

	[TestMethod]
	public async Task DisposeAsync_WithIAsyncDisposable_ShouldCallDisposeAsync()
	{
		// Arrange
		var mockAsyncDisposable = new MockAsyncDisposable();

		// Act
		await Disposer.DisposeAsync(mockAsyncDisposable);

		// Assert
		Assert.IsTrue(mockAsyncDisposable.IsAsyncDisposed);
	}

	[TestMethod]
	public async Task DisposeAsync_WithIDisposable_ShouldWork()
	{
		// Arrange
		var mockDisposable = new MockDisposable();

		// Act
		await Disposer.DisposeAsync(mockDisposable);

		// Assert
		Assert.IsTrue(mockDisposable.IsDisposed);
	}

	[TestMethod]
	public async Task DisposeAsync_WithNonDisposable_ShouldReturnCompleted()
	{
		// Arrange
		var notDisposable = new NotDisposable();

		// Act
		await Disposer.DisposeAsync(notDisposable);

		// Assert
		Assert.IsTrue(true);
	}

	private class MockDisposable : IDisposable
	{
		public bool IsDisposed { get; private set; }

	#region Interface IDisposable

		public void Dispose()
		{
			IsDisposed = true;
		}

	#endregion
	}

	private class MockAsyncDisposable : IAsyncDisposable
	{
		public bool IsAsyncDisposed { get; private set; }

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			IsAsyncDisposed = true;

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private class BothDisposable : IDisposable, IAsyncDisposable
	{
		public bool IsSyncDisposed { get; private set; }

		public bool IsAsyncDisposed { get; private set; }

	#region Interface IAsyncDisposable

		[ExcludeFromCodeCoverage]
		public ValueTask DisposeAsync()
		{
			IsAsyncDisposed = true;

			return ValueTask.CompletedTask;
		}

	#endregion

	#region Interface IDisposable

		public void Dispose()
		{
			IsSyncDisposed = true;
		}

	#endregion
	}

	private class NotDisposable
	{
		public string Value { get; set; } = "test";
	}
}