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

namespace Xtate.Persistence;

internal sealed class ServiceIdSetPersistingController : IDisposable
{
	private readonly Bucket       _bucket;
	private readonly ServiceIdSet _serviceIdSet;
	private          int          _record;

	public ServiceIdSetPersistingController(in Bucket bucket, ServiceIdSet serviceIdSet)
	{
		_bucket = bucket;
		_serviceIdSet = serviceIdSet ?? throw new ArgumentNullException(nameof(serviceIdSet));

		var shrink = serviceIdSet.Count > 0;
		while (true)
		{
			var recordBucket = bucket.Nested(_record);

			if (!recordBucket.TryGet(Key.Operation, out Key operation)
				|| !recordBucket.TryGetServiceId(Key.ServiceId, out var serviceId))
			{
				break;
			}

			switch (operation)
			{
				case Key.Added:
					_serviceIdSet.Add(serviceId);
					break;

				case Key.Removed:
					_serviceIdSet.Remove(serviceId);
					shrink = true;
					break;
			}

			_record ++;
		}

		if (shrink)
		{
			bucket.RemoveSubtree(Bucket.RootKey);

			_record = 0;
			foreach (var serviceId in _serviceIdSet)
			{
				var recordBucket = bucket.Nested(_record ++);
				recordBucket.Add(Key.ServiceId, serviceId);
				recordBucket.Add(Key.Operation, Key.Added);
			}
		}

		_serviceIdSet.Changed += OnChanged;
	}

#region Interface IDisposable

	public void Dispose()
	{
		_serviceIdSet.Changed -= OnChanged;
	}

#endregion

	private void OnChanged(ServiceIdSet.ChangedAction action, ServiceId serviceId)
	{
		switch (action)
		{
			case ServiceIdSet.ChangedAction.Add:
			{
				var bucket = _bucket.Nested(_record ++);
				bucket.AddServiceId(Key.ServiceId, serviceId);
				bucket.Add(Key.Operation, Key.Added);
				break;
			}

			case ServiceIdSet.ChangedAction.Remove:
				if (_serviceIdSet.Count == 0)
				{
					_record = 0;
					_bucket.RemoveSubtree(Bucket.RootKey);
				}
				else
				{
					var bucket = _bucket.Nested(_record ++);
					bucket.AddServiceId(Key.ServiceId, serviceId);
					bucket.Add(Key.Operation, Key.Removed);
				}

				break;

			default:
				throw Infra.Unexpected<Exception>(action);
		}
	}

	private enum Key
	{
		ServiceId,
		Operation,
		Added,
		Removed
	}
}