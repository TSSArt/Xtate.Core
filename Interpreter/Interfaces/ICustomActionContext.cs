using System;

namespace Xtate.CustomAction
{
	public interface ICustomActionContext
	{
		string Xml { get; }

		void AddValidationError<T>(string message, Exception? exception = default) where T : ICustomActionExecutor;

		ILocationAssigner RegisterLocationExpression(string expression);

		IExpressionEvaluator RegisterValueExpression(string expression);
	}
}