using System;

namespace Xtate
{
	public interface IExternalDataExpression : IEntity
	{
		Uri? Uri { get; }
	}
}