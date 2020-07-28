#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class SendBuilder : BuilderBase, ISendBuilder
	{
		private IContent?                           _content;
		private IValueExpression?                   _delayExpression;
		private int?                                _delayMs;
		private string?                             _event;
		private IValueExpression?                   _eventExpression;
		private string?                             _id;
		private ILocationExpression?                _idLocation;
		private ImmutableArray<ILocationExpression> _nameList;
		private ImmutableArray<IParam>.Builder?     _parameters;
		private Uri?                                _target;
		private IValueExpression?                   _targetExpression;
		private Uri?                                _type;
		private IValueExpression?                   _typeExpression;

		public SendBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface ISendBuilder

		public ISend Build() =>
				new SendEntity
				{
						Ancestor = Ancestor, EventName = _event, EventExpression = _eventExpression, Target = _target, TargetExpression = _targetExpression,
						Type = _type, TypeExpression = _typeExpression, Id = _id, IdLocation = _idLocation, DelayMs = _delayMs,
						DelayExpression = _delayExpression, NameList = _nameList, Parameters = _parameters?.ToImmutable() ?? default, Content = _content
				};

		public void SetEvent(string evt)
		{
			if (string.IsNullOrEmpty(evt)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(evt));

			_event = evt;
		}

		public void SetEventExpression(IValueExpression eventExpression) => _eventExpression = eventExpression ?? throw new ArgumentNullException(nameof(eventExpression));

		public void SetTarget(Uri target) => _target = target ?? throw new ArgumentNullException(nameof(target));

		public void SetTargetExpression(IValueExpression targetExpression) => _targetExpression = targetExpression ?? throw new ArgumentNullException(nameof(targetExpression));

		public void SetType(Uri type) => _type = type ?? throw new ArgumentNullException(nameof(type));

		public void SetTypeExpression(IValueExpression typeExpression) => _typeExpression = typeExpression ?? throw new ArgumentNullException(nameof(typeExpression));

		public void SetId(string id)
		{
			if (string.IsNullOrEmpty(id)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(id));

			_id = id;
		}

		public void SetIdLocation(ILocationExpression idLocation) => _idLocation = idLocation ?? throw new ArgumentNullException(nameof(idLocation));

		public void SetDelay(int delay)
		{
			if (delay < 0) throw new ArgumentOutOfRangeException(nameof(delay));

			_delayMs = delay;
		}

		public void SetDelayExpression(IValueExpression delayExpression) => _delayExpression = delayExpression ?? throw new ArgumentNullException(nameof(delayExpression));

		public void SetNameList(ImmutableArray<ILocationExpression> nameList)
		{
			if (nameList.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeEmptyList, nameof(nameList));

			_nameList = nameList;
		}

		public void AddParameter(IParam param)
		{
			if (param == null) throw new ArgumentNullException(nameof(param));

			(_parameters ??= ImmutableArray.CreateBuilder<IParam>()).Add(param);
		}

		public void SetContent(IContent content) => _content = content ?? throw new ArgumentNullException(nameof(content));

	#endregion
	}
}