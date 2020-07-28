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
using System.Diagnostics.CodeAnalysis;

namespace Xtate
{
	[Serializable]
	public sealed class SendId : LazyId
	{
		private SendId() { }

		private SendId(string val) : base(val) { }

		protected override string GenerateId() => IdGenerator.NewSendId(GetHashCode());

		public static SendId New() => new SendId();

		[return: NotNullIfNotNull("val")]
		public static SendId? FromString(string? val) => val != null ? new SendId(val) : null;
	}
}