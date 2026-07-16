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

using Xtate.DataTypes;
using Xtate.ExternalServices;
using Xtate.ExternalServices.HttpClient.Services;
using Xtate.ExternalServices.SmtpClient.Services;
using Xtate.StateMachineHost;

namespace Xtate.Test.UnitTests.ExternalServices;

[TestClass]
public class ExternalServiceProviderBaseCoverageTest
{
	[TestMethod]
	public async Task ProviderMatchesPrimaryAndAliasUriAndCreatesService()
	{
		var service = new TestExternalService();
		var provider = new TestExternalServiceProvider
					   {
						   ServiceFactoryFunc = () => new ValueTask<TestExternalService>(service)
					   };
		IExternalServiceProvider serviceProvider = provider;
		var primaryActivator = serviceProvider.TryGetActivator(new FullUri("urn:primary"));
		var aliasActivator = serviceProvider.TryGetActivator(new FullUri("urn:alias"));

		Assert.AreSame(provider, primaryActivator);
		Assert.AreSame(provider, aliasActivator);
		Assert.IsNull(serviceProvider.TryGetActivator(new FullUri("urn:other")));
		Assert.AreSame(service, await primaryActivator!.Create());

		IExternalServiceProvider noAlias = new NoAliasExternalServiceProvider
										   {
											   ServiceFactoryFunc = () => new ValueTask<TestExternalService>(service)
										   };
		Assert.IsNotNull(noAlias.TryGetActivator(new FullUri("urn:no-alias")));
		Assert.IsNull(noAlias.TryGetActivator(new FullUri("urn:alias")));
	}

	[TestMethod]
	public void BuiltInHttpAndSmtpProvidersMatchPrimaryAndAliasUris()
	{
		IExternalServiceProvider httpProvider = new HttpClientService.Provider
												{
													ServiceFactoryFunc = () => new ValueTask<HttpClientService>((HttpClientService) null!)
												};
		IExternalServiceProvider smtpProvider = new SmtpClientService.Provider
												{
													ServiceFactoryFunc = () => new ValueTask<SmtpClientService>((SmtpClientService) null!)
												};

		Assert.IsNotNull(httpProvider.TryGetActivator(new FullUri("http://xtate.net/scxml/service/#HTTPClient")));
		Assert.IsNotNull(httpProvider.TryGetActivator(new FullUri("http")));
		Assert.IsNull(httpProvider.TryGetActivator(new FullUri("smtp")));
		Assert.IsNotNull(smtpProvider.TryGetActivator(new FullUri("http://xtate.net/scxml/service/#SMTPClient")));
		Assert.IsNotNull(smtpProvider.TryGetActivator(new FullUri("smtp")));
		Assert.IsNull(smtpProvider.TryGetActivator(new FullUri("http")));
	}

	private sealed class TestExternalServiceProvider() : ExternalServiceProviderBase<TestExternalService>(type: "urn:primary", alias: "urn:alias");

	private sealed class NoAliasExternalServiceProvider() : ExternalServiceProviderBase<TestExternalService>(type: "urn:no-alias");

	private sealed class TestExternalService : IExternalService
	{
	#region Interface IExternalService

		public ValueTask<DataModelValue> GetResult() => new(new DataModelValue("result"));

	#endregion
	}
}