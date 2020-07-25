using System.Collections.Generic;

namespace Xtate
{
	internal sealed class KeyList<T>
	{
		public delegate void ChangeHandler(ChangedAction action, IEntity entity, List<T> list);

		public enum ChangedAction
		{
			Set
		}

		private readonly Dictionary<IEntity, List<T>> _dic = new Dictionary<IEntity, List<T>>();

		public event ChangeHandler? Changed;

		public void Set(IEntity entity, List<T> list)
		{
			_dic[entity] = list;

			Changed?.Invoke(ChangedAction.Set, entity, list);
		}

		public bool TryGetValue(IEntity entity, out List<T> list) => _dic.TryGetValue(entity, out list);
	}
}