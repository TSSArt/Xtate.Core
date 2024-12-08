// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Persistence;

internal sealed class InvokeIdSetPersistingController : IDisposable
{
	private const int InvokeId = 0;

	private const int Operation = 1;

	private const int Added = 2;

	private const int Removed = 3;

	private readonly Bucket _bucket;

	private readonly InvokeIdSet _invokeIdSet;

	private int _record;

	public InvokeIdSetPersistingController(in Bucket bucket, InvokeIdSet invokeIdSet)
	{
		_bucket = bucket;
		_invokeIdSet = invokeIdSet ?? throw new ArgumentNullException(nameof(invokeIdSet));

		var shrink = invokeIdSet.Count > 0;

		while (true)
		{
			var recordBucket = bucket.Nested(_record);

			if (!recordBucket.TryGet(Operation, out int operation)
				|| !recordBucket.TryGetServiceId(InvokeId, out var serviceId))
			{
				break;
			}

			switch (operation)
			{
				case Added:
					_invokeIdSet.Add((InvokeId)serviceId);

					break;

				case Removed:
					_invokeIdSet.Remove((InvokeId)serviceId);
					shrink = true;

					break;
			}

			_record ++;
		}

		if (shrink)
		{
			bucket.RemoveSubtree(Bucket.RootKey);

			_record = 0;

			foreach (var serviceId in _invokeIdSet)
			{
				var recordBucket = bucket.Nested(_record ++);
				recordBucket.Add(InvokeId, serviceId);
				recordBucket.Add(Operation, Added);
			}
		}

		_invokeIdSet.Changed += OnChanged;
	}

#region Interface IDisposable

	public void Dispose()
	{
		_invokeIdSet.Changed -= OnChanged;
	}

#endregion

	private void OnChanged(InvokeIdSet.ChangedAction action, InvokeId invokeId)
	{
		switch (action)
		{
			case InvokeIdSet.ChangedAction.Add:
			{
				var bucket = _bucket.Nested(_record ++);
				bucket.AddServiceId(InvokeId, invokeId);
				bucket.Add(Operation, Added);

				break;
			}

			case InvokeIdSet.ChangedAction.Remove:
				if (_invokeIdSet.Count == 0)
				{
					_record = 0;
					_bucket.RemoveSubtree(Bucket.RootKey);
				}
				else
				{
					var bucket = _bucket.Nested(_record ++);
					bucket.AddServiceId(InvokeId, invokeId);
					bucket.Add(Operation, Removed);
				}

				break;

			default:
				throw Infra.Unmatched(action);
		}
	}
}