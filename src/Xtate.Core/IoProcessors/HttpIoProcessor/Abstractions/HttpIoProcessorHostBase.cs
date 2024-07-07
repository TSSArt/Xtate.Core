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

namespace Xtate.IoProcessor;

public abstract class HttpIoProcessorHostBase<THost, TContext> where THost : HttpIoProcessorHostBase<THost, TContext>
{
	private ImmutableList<HttpIoProcessorBase<THost, TContext>> _processors = ImmutableList<HttpIoProcessorBase<THost, TContext>>.Empty;

	protected ImmutableList<HttpIoProcessorBase<THost, TContext>> Processors => _processors;

	private bool AddToList(HttpIoProcessorBase<THost, TContext> processor)
	{
		ImmutableList<HttpIoProcessorBase<THost, TContext>> preVal, newVal;
		do
		{
			preVal = Processors;

			if (preVal.Contains(processor))
			{
				return false;
			}

			newVal = preVal.Add(processor);
		}
		while (Interlocked.CompareExchange(ref _processors, newVal, preVal) != preVal);

		return preVal.Count == 0;
	}

	private bool RemoveFromList(HttpIoProcessorBase<THost, TContext> processor)
	{
		ImmutableList<HttpIoProcessorBase<THost, TContext>> preVal, newVal;
		do
		{
			preVal = Processors;

			var index = preVal.IndexOf(processor);
			if (index < 0)
			{
				return false;
			}

			newVal = preVal.RemoveAt(index);
		}
		while (Interlocked.CompareExchange(ref _processors, newVal, preVal) != preVal);

		return newVal.Count == 0;
	}

	public async ValueTask AddProcessor(HttpIoProcessorBase<THost, TContext> processor, CancellationToken token)
	{
		if (AddToList(processor))
		{
			try
			{
				await StartHost(token).ConfigureAwait(false);
			}
			catch
			{
				RemoveFromList(processor);

				throw;
			}
		}
	}

	public async ValueTask<bool> RemoveProcessor(HttpIoProcessorBase<THost, TContext> processor, CancellationToken token)
	{
		if (RemoveFromList(processor))
		{
			await StopHost(token).ConfigureAwait(false);

			return true;
		}

		return false;
	}

	protected abstract ValueTask StartHost(CancellationToken token);

	protected abstract ValueTask StopHost(CancellationToken token);
}