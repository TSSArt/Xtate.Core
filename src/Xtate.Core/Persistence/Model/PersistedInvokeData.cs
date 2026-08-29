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

using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Persistence;

public record PersistedInvokeData : InvokeData, IStoreSupport
{
	public PersistedInvokeData(Bucket bucket)
		: base(
			bucket.GetInvokeId(Key.InvokeId) ?? throw Infra.Fail<Exception>(),
			bucket.GetFullUri(Key.Type) ?? throw Infra.Fail<Exception>(),
			bucket.GetUri(Key.Source),
			bucket.GetString(Key.Body),
			bucket.GetDataModelValue(Key.Content),
			bucket.GetDataModelValue(Key.Parameters))
	{
		if (!bucket.TryGet(Key.TypeInfo, out TypeInfo storedTypeInfo) || storedTypeInfo != TypeInfo.InvokedService)
		{
			throw new ArgumentException(Resources.Exception_InvalidTypeInfoValue);
		}
	}

	public PersistedInvokeData(InvokeId InvokeId,
							   FullUri Type,
							   Uri? Source,
							   string? RawContent,
							   DataModelValue Content,
							   DataModelValue Parameters) : base(InvokeId, Type, Source, RawContent, Content, Parameters) { }

	public int RefId { get; set; }

#region Interface IStoreSupport

	public void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.InvokedService);
		bucket.AddId(Key.InvokeId, InvokeId);
		bucket.Add(Key.Type, Type);
		bucket.Add(Key.Source, Source);
		bucket.Add(Key.Body, RawContent);
		bucket.AddDataModelValue(Key.Content, Content);
		bucket.AddDataModelValue(Key.Parameters, Parameters);
	}

#endregion
}
