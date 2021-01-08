#region Copyright © 2019-2021 Sergii Artemenko

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

using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class ScriptNode : ExecutableEntityNode, IScript, IAncestorProvider
	{
		private readonly ScriptEntity _entity;

		public ScriptNode(in DocumentIdRecord documentIdNode, in ScriptEntity entity) : base(documentIdNode, (IScript?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IScript

		public IScriptExpression? Content => _entity.Content;

		public IExternalScriptExpression? Source => _entity.Source;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ScriptNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Content, Content);
			bucket.AddEntity(Key.Source, Source);
		}
	}
}