using System.Diagnostics.Contracts;

namespace Xtate
{
	internal interface IVisitorEntity<TEntity, in TIEntity> where TEntity : struct, IVisitorEntity<TEntity, TIEntity>, TIEntity
	{
		void Init(TIEntity source);

		[Pure]
		bool RefEquals(ref TEntity other);
	}
}