using System;

namespace Xtate.Builder
{
	public class CancelBuilder : BuilderBase, ICancelBuilder
	{
		private string?           _sendId;
		private IValueExpression? _sendIdExpression;

		public CancelBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface ICancelBuilder

		public ICancel Build() => new CancelEntity { Ancestor = Ancestor, SendId = _sendId, SendIdExpression = _sendIdExpression };

		public void SetSendId(string sendId)
		{
			if (string.IsNullOrEmpty(sendId)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(sendId));

			_sendId = sendId;
		}

		public void SetSendIdExpression(IValueExpression sendIdExpression) => _sendIdExpression = sendIdExpression ?? throw new ArgumentNullException(nameof(sendIdExpression));

	#endregion
	}
}