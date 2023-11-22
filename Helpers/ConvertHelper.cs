using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Xtate.Core
{
	internal static class ConvertHelper<TFrom, TTo>
	{
		public static readonly Func<TFrom, TTo> Convert = GetConverter();

		private static Func<TFrom, TTo> GetConverter()
		{
			var arg = Expression.Parameter(typeof(TFrom));

			return Expression.Lambda<Func<TFrom, TTo>>(arg, arg).Compile();
		}
	}}
