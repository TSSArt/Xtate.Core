namespace Xtate.Builder
{
	public interface ICancelBuilder
	{
		ICancel Build();

		void SetSendId(string sendId);
		void SetSendIdExpression(IValueExpression sendIdExpression);
	}
}