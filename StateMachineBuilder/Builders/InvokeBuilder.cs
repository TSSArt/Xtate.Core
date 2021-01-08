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

using System;
using System.Collections.Immutable;
using Xtate.Core;

namespace Xtate.Builder
{
	public class InvokeBuilder : BuilderBase, IInvokeBuilder
	{
		private bool                                _autoForward;
		private IContent?                           _content;
		private IFinalize?                          _finalize;
		private string?                             _id;
		private ILocationExpression?                _idLocation;
		private ImmutableArray<ILocationExpression> _nameList;
		private ImmutableArray<IParam>.Builder?     _parameters;
		private Uri?                                _source;
		private IValueExpression?                   _sourceExpression;
		private Uri?                                _type;
		private IValueExpression?                   _typeExpression;

		public InvokeBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IInvokeBuilder

		public IInvoke Build() =>
				new InvokeEntity
				{
						Ancestor = Ancestor, Type = _type, TypeExpression = _typeExpression, Source = _source, SourceExpression = _sourceExpression, Id = _id, IdLocation = _idLocation,
						NameList = _nameList, AutoForward = _autoForward, Parameters = _parameters?.ToImmutable() ?? default, Finalize = _finalize, Content = _content
				};

		public void SetType(Uri type) => _type = type ?? throw new ArgumentNullException(nameof(type));

		public void SetTypeExpression(IValueExpression typeExpression) => _typeExpression = typeExpression ?? throw new ArgumentNullException(nameof(typeExpression));

		public void SetSource(Uri source) => _source = source ?? throw new ArgumentNullException(nameof(source));

		public void SetSourceExpression(IValueExpression sourceExpression) => _sourceExpression = sourceExpression ?? throw new ArgumentNullException(nameof(sourceExpression));

		public void SetId(string id)
		{
			if (string.IsNullOrEmpty(id)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(id));

			_id = id;
		}

		public void SetIdLocation(ILocationExpression idLocation) => _idLocation = idLocation ?? throw new ArgumentNullException(nameof(idLocation));

		public void SetNameList(ImmutableArray<ILocationExpression> nameList)
		{
			if (nameList.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeEmptyList, nameof(nameList));

			_nameList = nameList;
		}

		public void SetAutoForward(bool autoForward) => _autoForward = autoForward;

		public void AddParam(IParam param)
		{
			if (param is null) throw new ArgumentNullException(nameof(param));

			(_parameters ??= ImmutableArray.CreateBuilder<IParam>()).Add(param);
		}

		public void SetFinalize(IFinalize finalize) => _finalize = finalize ?? throw new ArgumentNullException(nameof(finalize));

		public void SetContent(IContent content) => _content = content ?? throw new ArgumentNullException(nameof(content));

	#endregion
	}
}