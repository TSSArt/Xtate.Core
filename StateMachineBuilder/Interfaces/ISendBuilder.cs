using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public interface ISendBuilder
	{
		ISend Build();

		void SetEvent(string evt);
		void SetEventExpression(IValueExpression eventExpression);
		void SetTarget(Uri target);
		void SetTargetExpression(IValueExpression targetExpression);
		void SetType(Uri type);
		void SetTypeExpression(IValueExpression typeExpression);
		void SetId(string id);
		void SetIdLocation(ILocationExpression idLocation);
		void SetDelay(int delay);
		void SetDelayExpression(IValueExpression delayExpression);
		void SetNameList(ImmutableArray<ILocationExpression> nameList);
		void AddParameter(IParam param);
		void SetContent(IContent content);
	}
}