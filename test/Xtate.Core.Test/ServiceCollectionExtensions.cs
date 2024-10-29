using Xtate.IoC;

namespace Xtate.Core.Test
{
	public static class ServiceCollectionExtensions
	{
		public static Mock<T> AddMock<T>(this IServiceCollection services) where T : class
		{
			var mock = new Mock<T>();
			
			services.AddForwarding(_ => mock.Object);

			return mock;
		}

		public static Mock<T> AddMock<T>(this IServiceCollection services, Action<Mock<T>> configureMock) where T : class
		{
			var mock = new Mock<T>();

			configureMock(mock);

			services.AddForwarding(_ => mock.Object);

			return mock;
		}
	}
}
