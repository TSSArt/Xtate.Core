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

public class StateMachineGetter(IStateMachineService stateMachineService) : IAsyncInitialization
{
	private readonly AsyncInit<IStateMachine?> _stateMachineAsyncInit = AsyncInit.RunNow(stateMachineService, svc => svc.GetStateMachine());

	#region Interface IAsyncInitialization

	public Task Initialization => _stateMachineAsyncInit.Task;

#endregion

	[UsedImplicitly]
	public ValueTask<IStateMachine?> GetStateMachine() => new(_stateMachineAsyncInit.Value);

	//TODO:delete
	/*
		public async ValueTask<IStateMachine> GetStateMachine1(CancellationToken token)
		{
			if (_runtimeStateMachine is not null)
			{
				_stateMachineBase = _runtimeStateMachine.RuntimeStateMachine;
			}
			else if(_scxmlStateMachine is not null)
			{
				_stateMachineBase = await GetStateMachine(_scxmlStateMachine.Location, _scxmlStateMachine.ScxmlStateMachine).ConfigureAwait(false);
			}

			//var origin = _stateMachineStartOptions.Origin;

			//var location = _hostBaseUri?.HostBaseUri.CombineWith(origin.BaseUri);

			switch (origin.Type)
			{
				case StateMachineOriginType.StateMachine:
					_stateMachineBase = origin.AsStateMachine();
					break;

				case StateMachineOriginType.Scxml:
					_stateMachineBase = await GetStateMachine(location, origin.AsScxml()).ConfigureAwait(false);
					break;

				case StateMachineOriginType.Source:

					location = location.CombineWith(origin.AsSource());
					_stateMachineBase = await GetStateMachine(location, scxml: default).ConfigureAwait(false);
					break;

				default:
					throw new ArgumentException(Resources.Exception_StateMachineOriginMissed);
			}
		}
		*/ /*
		private async ValueTask<IStateMachine> CreateStateMachine(IScxmlStateMachine scxmlStateMachine)
		{
			var nameTable = new NameTable();
			var xmlResolver = new RedirectXmlResolver(_serviceLocator, _cancellationTokenSource.Token);
			var xmlParserContext = GetXmlParserContext(nameTable, uri);
			var xmlReaderSettings = GetXmlReaderSettings(nameTable, xmlResolver);
			var directorOptions = GetScxmlDirectorOptions(_serviceLocator, xmlParserContext, xmlReaderSettings, xmlResolver);

			using var xmlReader = scxml is null
				? XmlReader.Create(uri!.ToString(), xmlReaderSettings, xmlParserContext)
				: XmlReader.Create(new StringReader(scxml), xmlReaderSettings, xmlParserContext);

			var scxmlDirector = new ScxmlDirector(xmlReader, GetBuilderFactory(), directorOptions);

			return await scxmlDirector.ConstructStateMachine().ConfigureAwait(false);
		}*/
}